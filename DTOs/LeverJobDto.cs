using System;
using System.Text.Json.Serialization;

namespace JobAutoApply.API.DTOs;

public class LeverJobDto
{
    public string Id{get;set;} ="";
    public string Text{get;set;}="";
    public string HostedUrl{get;set;}="";
    public LeverCategoryDto? Categories{get;set;}
    [JsonPropertyName("descriptionBody")]
    public string? DescriptionBody { get; set; }
}

public class LeverCategoryDto
{
    public string? Location{get;set;}
    public string? Team{get;set;}
    public string? Commitment{get;set;}
}

public class LeverDescriptionDto
{
    public string? Plain{get;set;}
}