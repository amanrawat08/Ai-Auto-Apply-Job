using System;
using JobAutoApply.API.DTOs;

namespace JobAutoApply.API.Services;

public class LeverService
{
    private HttpClient _httpClient;
    public LeverService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<LeverJobDto>> GetJobsAsync(string companyToken)
    {
        string url = $"https://api.lever.co/v0/postings/{Uri.EscapeDataString(companyToken)}?mode=json";

        var jobs = await _httpClient.GetFromJsonAsync<List<LeverJobDto>>(url);

        return jobs ?? new List<LeverJobDto>();
    }
}
