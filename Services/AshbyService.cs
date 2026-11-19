using System;
using JobAutoApply.API.DTOs;

namespace JobAutoApply.API.Services;

public class AshbyService
{
    private readonly HttpClient _httpClient;
    public AshbyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AshbyResponseDto?> GetJobsAsync(string boardName)
    {
        var url = $"https://api.ashbyhq.com/posting-api/job-board/{Uri.EscapeDataString(boardName)}";
        return await _httpClient.GetFromJsonAsync<AshbyResponseDto>(url);
    }
}
