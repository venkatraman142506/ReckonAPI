using Microsoft.AspNetCore.Mvc;
using ReckonAPI.Models;
using ReckonAPI.Services;
using ReckonAPI.Utils;

namespace ReckonAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IApiService _apiService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(IApiService apiService, ILogger<SearchController> logger)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("search")]
        public async Task<IActionResult> PerformSearch()
        {
            try
            {
                var textToSearch = await _apiService.GetTextToSearchAsync();
                var subTexts = await _apiService.GetSubTextsAsync();

                if (textToSearch == null || string.IsNullOrEmpty(textToSearch.text))
                {
                    _logger.LogError("textToSearch is null or empty.");
                    return StatusCode(500, "Error fetching text to search.");
                }

                if (subTexts == null || subTexts.subTexts == null || !subTexts.subTexts.Any())
                {
                    _logger.LogError("subTexts is null or empty.");
                    return StatusCode(500, "Error fetching subtexts.");
                }

                var results = new List<SearchResult>();

                foreach (var subText in subTexts.subTexts)
                {
                    var positions = StringSearchUtility.FindAllOccurrences(textToSearch.text, subText);
                    var result = positions.Any() ? string.Join(", ", positions) : "<No Output>";
                    results.Add(new SearchResult
                    {
                        Subtext = subText,
                        Result = result
                    });
                }

                var payload = new SearchResultPayload
                {
                    Candidate = "Your Name",
                    Text = textToSearch.text,
                    Results = results
                };

                await _apiService.PostResultsAsync(payload);

                return Ok(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while performing the search.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}