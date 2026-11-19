using System;
using Hangfire.Common;
using JobAutoApply.API.DTOs;
using JobAutoApply.API.Interface;
using JobAutoApply.API.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JobAutoApply.API.Providers;

public class GreenhouseJobProvider : IJobProvider
{
    private readonly GreenhouseService _greenhouseService;
    public string PlatformName => "Greenhouse";

    public GreenhouseJobProvider(GreenhouseService greenhouseService)
    {
        _greenhouseService = greenhouseService;
    }

    public async Task<List<NormalizedJobDto>> GetJobsAsync(string boardToken)
    {
        var response = await  _greenhouseService.GetJobAsync(boardToken);
        if(response?.Jobs==null)
        {
            return new List<NormalizedJobDto>();
        }

        var normalizedJobs = response.Jobs.Select(
            j=> new NormalizedJobDto
            {
                ExternalJobId = j.Id.ToString(),
                Source = PlatformName,
                Title = j.Title,
                Company = boardToken,
                Location = j?.Location?.Name ?? "Unknown",
                Description = j.Content,
                EmploymentType = null,
                Category = null,
                JobUrl = j.JobUrl,
                ApplyUrl = j.JobUrl,
                IsRemote = null
            }
        ).ToList();

        return normalizedJobs;
    }
}
