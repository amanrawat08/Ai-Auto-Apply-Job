using System.Text.Json.Serialization;

namespace JobAutoApply.API.DTOs;

public class GreenhouseResponseDto
{
    [JsonPropertyName("jobs")]
    public List<GreenhouseJobDto> Jobs {get;set;} = new();
}

public class GreenhouseJobDto
{
    [JsonPropertyName("id")]
    public long Id{get;set;}
    [JsonPropertyName("title")]
    public string Title {get;set;}="";
    [JsonPropertyName("location")]
    public GreenhouseLocationDto? Location{get;set;}
    [JsonPropertyName("absolute_url")]
    public string JobUrl{get;set;} ="";
    [JsonPropertyName("content")]
    public string? Content{get;set;}

}

public class GreenhouseLocationDto
{
    [JsonPropertyName("name")]
    public string Name{get;set;} = "";
}
