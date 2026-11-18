using System;

namespace JobAutoApply.API.Models;

public class JobSource
{   
    public int Id{get;set;}
    public string CompanyName{get;set;}="";
    public string Platform{get;set;}="";
    public string BoardToken{get;set;}="";
    public bool IsActive{get;set;}=true;
    public DateTime? LastSyncedAt{get;set;}

}
