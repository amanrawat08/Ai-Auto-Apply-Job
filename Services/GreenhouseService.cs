using JobAutoApply.API.DTOs;

namespace JobAutoApply.API.Services;

public class GreenhouseService
{
    private readonly HttpClient _http;

    public GreenhouseService(HttpClient http)
    {
        _http = http;
    }

    public async Task<GreenhouseResponseDto?> GetJobAsync(string boardToken)
    {
        var url = $"https://boards-api.greenhouse.io/v1/boards/{Uri.EscapeDataString(boardToken)}/jobs?content=true";

        return await _http.GetFromJsonAsync<GreenhouseResponseDto>(url);

    }

}
