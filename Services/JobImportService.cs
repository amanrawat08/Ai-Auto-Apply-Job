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
}
