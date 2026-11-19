using JobAutoApply.API.Data;
using JobAutoApply.API.DTOs;
using JobAutoApply.API.Models;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;

namespace JobAutoApply.API.Services;

public class JobImportService
{
    private readonly ApplicationDbContext _context;
    private readonly GreenhouseService _greenHouse;
    private readonly LeverService _leverService;
    private readonly AshbyService _ashbyService;

    public JobImportService(ApplicationDbContext context, GreenhouseService greenhouse, LeverService leverService, AshbyService ashbyService)
    {
        _context = context;
        _greenHouse = greenhouse;
        _leverService = leverService;
        _ashbyService = ashbyService;
    }

    public async Task<int> ImportJobAsync(string boardToken)
    {
        var response = await _greenHouse.GetJobAsync(boardToken);

        if (response == null) return 0;

        var jobs = response.Jobs;

        var existingIds = (
            await _context.Jobs.Where(j => j.Source == "Greenhouse").Select(j => j.ExternalJobId).ToListAsync()
        ).ToHashSet();


        var newJobs = new List<Job>();

        foreach (var item in jobs)
        {
            string externalId = item.Id.ToString();

            if (existingIds.Contains(externalId)) continue;

            newJobs.Add(
                new Job
                {
                    ExternalJobId = externalId,
                    Source = "Greenhouse",
                    Title = item.Title,
                    Company = boardToken,
                    Location = item.Location?.Name ?? "Unknown",
                    Description = item.Content,
                    JobUrl = item.JobUrl
                }
            );

            existingIds.Add(externalId);
        }
        _context.Jobs.AddRange(newJobs);

        await _context.SaveChangesAsync();

        return newJobs.Count();


    }


    public async Task<object> ImportAllJobsAsync()
    {
        var sources = await _context.JobSources.
        Where(j => j.IsActive).ToListAsync();

        int totalImported = 0;
        int successfullSources = 0;

        var failedSources = new List<string>();

        foreach (var source in sources)
        {
            try
            {
                int imported = source.Platform.ToLowerInvariant()
                switch
                {
                    "greenhouse" => await ImportJobAsync(source.BoardToken),
                    "lever" => await ImportLeverJobsAsync(source.BoardToken),
                    "ashby" => await ImportAshbyJobsAsync(source.BoardToken),
                    _ => throw new NotSupportedException(
                    $"Unsupported platform: {source.Platform}")
                };
                totalImported += imported;
                successfullSources++;

                source.LastSyncedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                failedSources.Add(source.CompanyName);

                Console.WriteLine(
                    $"Import failed for {source.CompanyName}");

                Console.WriteLine(ex.ToString());
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

    // Save Lever Jobs in SQL Server
    public async Task<int> ImportLeverJobsAsync(string companyToken)
    {
        var response = await _leverService.GetJobsAsync(companyToken);

        var externalsId = response.Select(j => j.Id).ToList();

        var existingIds = (await _context.Jobs.Where(
            j => j.Source == "Lever" && externalsId.Contains(j.ExternalJobId)
        ).Select(j => j.ExternalJobId).ToListAsync()).ToHashSet();


        var newJobs = new List<Job>();

        foreach (var item in response)
        {
            if (existingIds.Contains(item.Id)) continue;

            newJobs.Add(new Job
            {
                ExternalJobId = item.Id,
                Source = "Lever",
                Title = item.Text,
                Company = companyToken,
                Location = item.Categories?.Location ?? "Unknown",
                Description = item.DescriptionBody,
                EmployementType = item.Categories?.Commitment,
                Category = item.Categories?.Team,
                JobUrl = item.HostedUrl
            });

            existingIds.Add(item.Id);
        }

        _context.Jobs.AddRange(newJobs);
        await _context.SaveChangesAsync();
        return newJobs.Count;

    }

    // Save AshbyJob in sql server

    public async Task<int> ImportAshbyJobsAsync(string boardName)
    {
        var response = await _ashbyService.GetJobsAsync(boardName);

        if (response == null) return 0;

        var jobs = response.Jobs;

        var externalIds = jobs
            .Select(j => j.Id)
            .ToList();

        var existingIds = (await _context.Jobs
            .Where(j =>
                j.Source == "Ashby" &&
                externalIds.Contains(j.ExternalJobId))
            .Select(j => j.ExternalJobId)
            .ToListAsync())
            .ToHashSet();

        var newJobs = new List<Job>();

        foreach (var item in jobs)
        {
            // Skip jobs already in SQL Server
            if (existingIds.Contains(item.Id))
                continue;

            newJobs.Add(new Job
            {
                ExternalJobId = item.Id,
                Source = "Ashby",
                Title = item.Title,
                Company = boardName,
                Location = item.Location,
                Description = item.DescriptionHtml,
                JobUrl = item.JobUrl,
                EmployementType = item.EmployementType,
                Category = item.Department
            });

            // Prevent duplicates within the same batch
            existingIds.Add(item.Id);
        }

        Console.WriteLine($"Ashby jobs fetched: {jobs.Count}");
        Console.WriteLine($"New Ashby jobs: {newJobs.Count}");

        _context.Jobs.AddRange(newJobs);

        await _context.SaveChangesAsync();

        return newJobs.Count;
    }


}
