using System;
using JobAutoApply.API.DTOs;
using JobAutoApply.API.Interface;
using JobAutoApply.API.Services;

namespace JobAutoApply.API.Providers;

public class LeverJobProvider : IJobProvider
{
    
    public string PlatformName => "Lever";
    private LeverService _leverService;
    public LeverJobProvider(LeverService leverService)
    {
        _leverService = leverService;
    }

    public async Task<List<NormalizedJobDto>> GetJobsAsync(string boardToken)
    {
        var response = await _leverService.GetJobsAsync(boardToken);

        if(response == null)
        {
            return new List<NormalizedJobDto>();
        }

        var normalizedJobs = response.Select(
            j=>new NormalizedJobDto
            {
                ExternalJobId = j.Id,
                Source = PlatformName,
                Title = j.Text,
                Company = boardToken,
                Location = j.Categories?.Location ?? "Unknown",
                EmploymentType = j.Categories?.Commitment,
                Description = j.DescriptionBody,
                Category = j.Categories?.Team,
                JobUrl = j.HostedUrl,
                ApplyUrl = j.HostedUrl,
                IsRemote = null
            }
        ).ToList();
        return normalizedJobs;
    }
}
