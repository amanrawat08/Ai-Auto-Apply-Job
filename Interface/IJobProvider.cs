using System;
using JobAutoApply.API.DTOs;

namespace JobAutoApply.API.Interface;

public interface IJobProvider
{
    string PlatformName { get; }

    Task<List<NormalizedJobDto>> GetJobsAsync(string boardToken);
}
