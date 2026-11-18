using JobAutoApply.API.Data;
using JobAutoApply.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAutoApply.API.Services;

public class JobImportService
{
    private readonly ApplicationDbContext _context;
    private readonly GreenhouseService _greenHouse;

    public JobImportService(ApplicationDbContext context, GreenhouseService greenhouse)
    {
        _context = context;
        _greenHouse = greenhouse;
    }

    public async Task<int> ImportJobAsync(string boardToken)
    {
        var response = await _greenHouse.GetJobAsync(boardToken);

        if (response == null) return 0;

        var jobs = response.Jobs.Where(
            j => j.Location?.Name.Contains("India", StringComparison.OrdinalIgnoreCase) == true
        );

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
        Where(j => j.Platform == "Greenhouse" && j.IsActive).ToListAsync();

        int totalImported = 0;
        int successfullSources = 0;

        var failedSources = new List<string>();

        foreach (var source in sources)
        {
            try
            {
                int imported = await ImportJobAsync(source.BoardToken);
                totalImported += imported;
                successfullSources++;

                source.LastSyncedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                failedSources.Add(source.CompanyName);
                // Log errors instead of silently ignoring them
                Console.WriteLine(
                    $"Failed to import {source.CompanyName}: {ex.Message}");
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
}
