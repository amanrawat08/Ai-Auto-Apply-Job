using System;

namespace JobAutoApply.API.DTOs;

public class NormalizedJobDto
{
    public string ExternalJobId{get;set;}="";
    public string Source{get;set;}="";
    public string Title{get;set;}="";
    public string Company{get;set;}="";
    public string Location{get;set;}="Unknown";
    public string? Description{get;set;}
    public string? EmploymentType{get;set;}
    public string? Category{get;set;}
    public string JobUrl{get;set;}="";
    public string? ApplyUrl{get;set;}
    public bool? IsRemote{get;set;}
}
