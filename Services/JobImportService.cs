
using JobAutoApply.API.Data;
using JobAutoApply.API.Interface;
using JobAutoApply.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAutoApply.API.Services;

public class JobImportService
{
    private readonly ApplicationDbContext _context;
    private readonly IEnumerable<IJobProvider> _providers;
    private readonly ILogger<JobImportService> _logger;

    public JobImportService(
        ApplicationDbContext context,
        IEnumerable<IJobProvider> jobProviders,
        ILogger<JobImportService> logger)
    {
        _context = context;
        _providers = jobProviders;
        _logger = logger;
    }

    // Import jobs from all active sources
    public async Task<object> ImportAllJobsAsync()
    {
        var sources = await _context.JobSources
            .Where(j => j.IsActive)
            .ToListAsync();

        int totalImported = 0;
        int successfullSources = 0;

        var failedSources = new List<string>();

        foreach (var source in sources)
        {
            try
            {
                // Find matching ATS provider
                var provider = _providers.FirstOrDefault(
                    p => p.PlatformName.Equals(
                        source.Platform,
                        StringComparison.OrdinalIgnoreCase)
                );

                if (provider == null)
                {
                    throw new NotSupportedException(
                        $"Provider {source.Platform} not registered."
                    );
                }

                // Import jobs using common method
                int imported = await ImportJobsAsync(
                    provider,
                    source.BoardToken
                );

                totalImported += imported;
                successfullSources++;

                // Update successful sync timestamp
                source.LastSyncedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                failedSources.Add(source.CompanyName);

                _logger.LogError(
                    ex,
                    "Import failed for {Company} on {Platform}",
                    source.CompanyName,
                    source.Platform
                );
            }
        }

        return new
        {
            TotalSources = sources.Count,
            SuccessfullSources = successfullSources,
            FailedSources = failedSources,
            TotalImported = totalImported
        };
    }

    // Common import method for all ATS providers
    public async Task<int> ImportJobsAsync(IJobProvider provider,string boardToken)
    {
        // Step 1: Fetch normalized jobs
        var jobs = await provider.GetJobsAsync(boardToken);

        if (jobs == null || jobs.Count == 0)
        {
            return 0;
        }

        // Step 2: Collect valid external IDs
        var externalIds = jobs
            .Where(j => !string.IsNullOrWhiteSpace(j.ExternalJobId))
            .Select(j => j.ExternalJobId)
            .Distinct()
            .ToList();

        // Step 3: Check existing jobs in SQL Server
        var existingIds = (
            await _context.Jobs
                .Where(j =>
                    j.Source == provider.PlatformName &&
                    externalIds.Contains(j.ExternalJobId))
                .Select(j => j.ExternalJobId)
                .ToListAsync()
        ).ToHashSet();

        // Step 4: Prepare new jobs
        var newJobs = new List<Job>();

        foreach (var item in jobs)
        {
            if (string.IsNullOrWhiteSpace(item.ExternalJobId))
            {
                continue;
            }

            if (existingIds.Contains(item.ExternalJobId))
            {
                continue;
            }

            var newJob = new Job
            {
                ExternalJobId = item.ExternalJobId,
                Source = provider.PlatformName,
                Title = item.Title,
                Company = item.Company,
                Location = item.Location,
                Description = item.Description,
                EmployementType = item.EmploymentType,
                Category = item.Category,
                JobUrl = item.JobUrl
            };

            newJobs.Add(newJob);

            // Prevent duplicates in same API response
            existingIds.Add(item.ExternalJobId);
        }

        // Step 5: Save only new jobs
        if (newJobs.Count > 0)
        {
            _context.Jobs.AddRange(newJobs);

            await _context.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Imported {Count} jobs from {Platform}/{BoardToken}",
            newJobs.Count,
            provider.PlatformName,
            boardToken
        );

        return newJobs.Count;
    }
}
