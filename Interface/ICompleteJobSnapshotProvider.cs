using JobAutoApply.API.DTOs;

namespace JobAutoApply.API.Interface;

public interface ICompleteJobSnapshotProvider:IJobProvider
{
    Task<JobFetchResultDto> GetSnapshotAsync(string boardToken);
}
