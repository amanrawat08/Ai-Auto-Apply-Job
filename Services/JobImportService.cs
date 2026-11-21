
using JobAutoApply.API.Data;
using JobAutoApply.API.DTOs;
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
    public async Task<int> ImportJobsAsync(IJobProvider provider, string boardToken)
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

    // Implement the synchronization of getting jobs
    /* public async Task<JobSyncResultDto> SyncJobsAsync(IJobProvider provider, string boardToken)
     {
         var jobs = await provider.GetJobsAsync(boardToken);
         if(jobs==null)
         {
             throw new InvalidOperationException("Provider returned a null response");
         }

         var now= DateTime.UtcNow;

         var result = new JobSyncResultDto
         {
             Platform = provider.PlatformName,
             BoardToken = boardToken,
             Fetched = jobs.Count,
             SyncedAt = now  
         };
         // Validate incoming job IDs
         if (jobs.Any(j => string.IsNullOrWhiteSpace(j.ExternalJobId)))
         {
             throw new InvalidOperationException("Provider returned jobs with missing IDs.");
         }

         // Reject duplicate external IDs in one response
         var duplicateIds = jobs.GroupBy(j=>j.ExternalJobId)
         .Where(j=>j.Count() > 1).Select(j=>j.Key).ToList();

         if (duplicateIds.Count > 0)
         {
             throw new InvalidOperationException(
             "Provider returned duplicate job IDs.");
         }

         var externalIds = jobs.Select(j=>j.ExternalJobId).ToList();

         // Query only jobs belonging to this ATS source
         var existingJobs = await _context.Jobs.Where(
             j=>j.Source==provider.PlatformName && externalIds.Contains(j.ExternalJobId)
         ).ToListAsync();


         var existingLookup = existingJobs.ToDictionary(j=>j.ExternalJobId);

         foreach (var item in jobs)
         {
             if(!existingLookup.TryGetValue(item.ExternalJobId, out var existingJob))
             {
                 // Insert
                 var newjob = new Job
                 {
                     ExternalJobId = item.ExternalJobId,
                     Source = provider.PlatformName,
                     Title = item.Title,
                     Company = item.Company,
                     Location = item.Location,
                     Description = item.Description,
                     EmployementType = item.EmploymentType,
                     Category = item.Category,
                     JobUrl = item.JobUrl,
                     IsActive = true,
                     CreateAt = now,
                     UpdatedAt = now,
                     LastSeenAt = now  
                 };

                 _context.Jobs.Add(newjob);
                 result.Inserted++;
                 continue;
             }

             // Updated
             bool changed = existingJob.Title!=item.Title||
             existingJob.Company!=item.Company||
             existingJob.Location!=item.Location||
             existingJob.Description!=item.Description||
             existingJob.EmployementType!=item.EmploymentType||
             existingJob.Category!=item.Category||
             existingJob.JobUrl!=item.JobUrl;

             if (changed)
             {
                 existingJob.Title = item.Title;
                 existingJob.Company = item.Company;
                 existingJob.Location= item.Location;
                 existingJob.Description= item.Description;
                 existingJob.EmployementType = item.EmploymentType;
                 existingJob.Category = item.Category;
                 existingJob.JobUrl= item.JobUrl;
                 existingJob.UpdatedAt= now;
                 result.Updated++;
             }

             if (!existingJob.IsActive)
             {
                 existingJob.IsActive = true;
                 existingJob.ClosedAt = null;
                 existingJob.UpdatedAt = now;
                 result.Reactivated++;
             }

             existingJob.LastSeenAt = now;
         }

         await _context.SaveChangesAsync();


         return result;

     }
     */

    public async Task<JobSyncResultDto> SyncJobsAsync(IJobProvider provider, string boardToken)
    {
        // 1. Fetch jobs with snapshot metadata if supported
        JobFetchResultDto snapshot;

        if (provider is ICompleteJobSnapshotProvider snapshotProvider)
        {
            snapshot = await snapshotProvider.GetSnapshotAsync(boardToken);
        }
        else
        {
            snapshot = new JobFetchResultDto
            {
                Jobs = await provider.GetJobsAsync(boardToken),
                IsCompleteSnapshot = false,
                FetchedAt = DateTime.UtcNow
            };
        }

        if (snapshot == null || snapshot.Jobs == null)
        {
            throw new InvalidOperationException(
                "Provider returned an invalid snapshot.");
        }

        var jobs = snapshot.Jobs;
        var now = DateTime.UtcNow;

        var result = new JobSyncResultDto
        {
            Platform = provider.PlatformName,
            BoardToken = boardToken,
            Fetched = jobs.Count,
            SyncedAt = now
        };

        // 2. Validate IDs before modifying SQL Server
        if (jobs.Any(j =>
            string.IsNullOrWhiteSpace(j.ExternalJobId)))
        {
            throw new InvalidOperationException(
                "Provider returned a job without an external ID.");
        }

        var duplicateIds = jobs
            .GroupBy(j => j.ExternalJobId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
        {
            throw new InvalidOperationException(
                "Provider returned duplicate external IDs.");
        }

        // 3. Fetch only jobs belonging to this board
        var existingBoardJobs = await _context.Jobs
            .Where(j =>
                j.Source == provider.PlatformName &&
                j.BoardToken == boardToken)
            .ToListAsync();

        var existingLookup = existingBoardJobs
            .ToDictionary(j => j.ExternalJobId);

        var incomingIds = jobs
            .Select(j => j.ExternalJobId)
            .ToHashSet();

        // 4. Reject IDs already owned by another board
        // under the existing (Source, ExternalJobId) index
        var conflictingIds = await _context.Jobs
            .Where(j =>
                j.Source == provider.PlatformName &&
                incomingIds.Contains(j.ExternalJobId) &&
                j.BoardToken != boardToken)
            .Select(j => j.ExternalJobId)
            .ToListAsync();

        if (conflictingIds.Count > 0)
        {
            throw new InvalidOperationException(
                "One or more jobs belong to a different board.");
        }

        // 5. Insert, update, reactivate and reset misses
        foreach (var item in jobs)
        {
            if (!existingLookup.TryGetValue(
                item.ExternalJobId,
                out var existingJob))
            {
                var newJob = new Job
                {
                    ExternalJobId = item.ExternalJobId,
                    Source = provider.PlatformName,
                    BoardToken = boardToken,
                    Title = item.Title,
                    Company = item.Company,
                    Location = item.Location,
                    Description = item.Description,
                    EmployementType = item.EmploymentType,
                    Category = item.Category,
                    JobUrl = item.JobUrl,
                    IsActive = true,
                    CreateAt = now,
                    UpdatedAt = now,
                    LastSeenAt = now,
                    ConsecutiveMisses = 0,
                    FirstMissingAt = null
                };

                _context.Jobs.Add(newJob);
                result.Inserted++;
                continue;
            }

            bool changed =
                existingJob.Title != item.Title ||
                existingJob.Company != item.Company ||
                existingJob.Location != item.Location ||
                existingJob.Description != item.Description ||
                existingJob.EmployementType != item.EmploymentType ||
                existingJob.Category != item.Category ||
                existingJob.JobUrl != item.JobUrl;

            if (changed)
            {
                existingJob.Title = item.Title;
                existingJob.Company = item.Company;
                existingJob.Location = item.Location;
                existingJob.Description = item.Description;
                existingJob.EmployementType = item.EmploymentType;
                existingJob.Category = item.Category;
                existingJob.JobUrl = item.JobUrl;
                existingJob.UpdatedAt = now;

                result.Updated++;
            }

            if (!existingJob.IsActive)
            {
                existingJob.IsActive = true;
                existingJob.ClosedAt = null;
                existingJob.UpdatedAt = now;

                result.Reactivated++;
            }

            if (existingJob.ConsecutiveMisses > 0 ||
                existingJob.FirstMissingAt != null)
            {
                existingJob.ConsecutiveMisses = 0;
                existingJob.FirstMissingAt = null;

                result.MissingReset++;
            }

            existingJob.LastSeenAt = now;
        }

        // 6. Decide whether closure detection is safe
        bool canCheckClosures =
            snapshot.IsCompleteSnapshot &&
            jobs.Count > 0;

        if (!canCheckClosures)
        {
            result.ClosureCheckSkipped = true;
            result.ClosureSkipReason =
                !snapshot.IsCompleteSnapshot
                    ? "Snapshot completeness not verified"
                    : "Empty snapshot requires separate verification";
        }
        else
        {
            // 7. Count missing jobs from this board only
            var missingJobs = existingBoardJobs
                .Where(j =>
                    j.IsActive &&
                    !incomingIds.Contains(j.ExternalJobId))
                .ToList();

            foreach (var missingJob in missingJobs)
            {
                missingJob.ConsecutiveMisses++;

                missingJob.FirstMissingAt ??= now;

                result.MissingDetected++;

                // Close after 3 consecutive verified misses
                if (missingJob.ConsecutiveMisses >= 3)
                {
                    missingJob.IsActive = false;
                    missingJob.ClosedAt = now;
                    missingJob.UpdatedAt = now;

                    result.Closed++;
                }
            }
        }

        // 8. Persist all changes together
        await _context.SaveChangesAsync();

        return result;

    }

}
