using System;

namespace JobAutoApply.API.Models;

public class Job
{
    public int Id {get;set;}
    public string ExternalJobId {get;set;} = string.Empty;
    public string Source {get;set;} = string.Empty;
    public string Title {get;set;} = string.Empty;
    public string Company {get;set;} = string.Empty;
    public string Location {get;set;} = string.Empty;
    public string? Description  {get;set;}
    public string? EmployementType {get;set;}
    public string? Category {get;set;}
    public string? JobUrl {get;set;} = string.Empty;
    public DateTime? CreateAt {get;set;} = DateTime.UtcNow;

    public bool IsActive{get;set;} = true;
    public string? BoardToken { get; set; }
    public DateTime? LastSeenAt{get;set;}
    public DateTime? ClosedAt{get;set;}
    public DateTime? UpdatedAt{get;set;} = DateTime.UtcNow;

    public int ConsecutiveMisses {get;set;} = 0;
    public DateTime? FirstMissingAt{get;set;}

}
