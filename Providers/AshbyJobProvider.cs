using System;
using Hangfire.Common;
using JobAutoApply.API.DTOs;
using JobAutoApply.API.Interface;
using JobAutoApply.API.Services;

namespace JobAutoApply.API.Providers;

public class AshbyJobProvider : IJobProvider
{
    private AshbyService _ashbyService;
    public string PlatformName => "Ashby";

    public AshbyJobProvider(AshbyService ashbyService)
    {
        _ashbyService = ashbyService;
    }

    public async Task<List<NormalizedJobDto>> GetJobsAsync(string boardToken)
    {
        var response = await _ashbyService.GetJobsAsync(boardToken);

        if (response?.Jobs == null) { return new List<NormalizedJobDto>();}

        var normalizedJobs = response.Jobs.Select(
            jobs=> new NormalizedJobDto
            {
                EmploymentType= jobs.EmployementType,
                ExternalJobId = jobs.Id,
                Source = PlatformName,
                Title = jobs.Title,
                Company = boardToken,
                Location = jobs.Location ?? "Unknown",
                Description = jobs.DescriptionHtml,
                Category = jobs.Department,
                JobUrl = jobs.JobUrl,
                ApplyUrl = jobs.ApplyUrl,
                IsRemote = jobs.IsRemote
            }
        ).ToList();

        return normalizedJobs;
    }
}
