using System.Net.Http;
using System.Text.Json;
using ReckonAPI.Models;
using Microsoft.Extensions.Logging;

namespace ReckonAPI.Services
{
    public interface IApiService
    {
        Task<TextToSearch> GetTextToSearchAsync();
        Task<SubTexts> GetSubTextsAsync();
        Task PostResultsAsync(SearchResultPayload payload);

    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TextToSearch> GetTextToSearchAsync()
        {
            return await FetchFromApi<TextToSearch>("https://join.reckon.com/test2/textToSearch");
        }

        public async Task<SubTexts> GetSubTextsAsync()
        {
            return await FetchFromApi<SubTexts>("https://join.reckon.com/test2/subTexts");
        }

        public async Task PostResultsAsync(SearchResultPayload payload)
        {
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://join.reckon.com/test2/submitResults", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task<T> FetchFromApi<T>(string url)
        {
            const int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(jsonString);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, $"Error fetching data from {url}. Retrying...");
                    await Task.Delay(500); // Wait for a moment before retrying
                }
            }

            // If retries are exhausted, throw an exception
            throw new InvalidOperationException($"Failed to fetch data from {url} after {maxRetries} retries.");

        }
    }
}