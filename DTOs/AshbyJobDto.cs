using System;
using System.Text.Json.Serialization;

namespace JobAutoApply.API.DTOs;

public class AshbyResponseDto
{
    [JsonPropertyName("jobs")]
    public List<AshbyJobDto> Jobs{get;set;} = new(); 
}

public class AshbyJobDto
{
    public string Id{get;set;} = "";
    public string Title{get;set;} = "";
    public string Location{get;set;} = "";
    public string? Department{get;set;}
    public string? EmployementType{get;set;}
    public string? WorkplaceType{get;set;}
    public string? DescriptionHtml{get;set;}
    public string JobUrl{get;set;} = "";
    public string ApplyUrl{get;set;} = "";
    public bool? IsRemote{get;set;}
}
