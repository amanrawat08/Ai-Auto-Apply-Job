using System;

namespace JobAutoApply.API.DTOs;

public class JobFetchResultDto
{
    public List<NormalizedJobDto> Jobs{get;set;} = new();
    public bool IsCompleteSnapshot{get;set;}
    public DateTime FetchedAt{get;set;} = DateTime.UtcNow;
}
