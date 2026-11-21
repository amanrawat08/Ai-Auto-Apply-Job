
using JobAutoApply.API.DTOs;
using JobAutoApply.API.Interface;

namespace JobAutoApply.API.Providers;

public class FakeJobProvider : ICompleteJobSnapshotProvider
{
    public string PlatformName => "TestATS";

    public List<NormalizedJobDto> Jobs { get; set; } = new();

    public bool IsComplete { get; set; } = true;

    public Task<List<NormalizedJobDto>> GetJobsAsync(
        string boardToken)
    {
        return Task.FromResult(Jobs.ToList());
    }

    public Task<JobFetchResultDto> GetSnapshotAsync(
        string boardToken)
    {
        return Task.FromResult(new JobFetchResultDto
        {
            Jobs = Jobs.ToList(),
            IsCompleteSnapshot = IsComplete,
            FetchedAt = DateTime.UtcNow
        });
    }
}
