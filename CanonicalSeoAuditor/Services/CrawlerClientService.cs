using System.Net.Http;
using System.Threading.Tasks;
using CanonicalSeoAuditor.Models;
using Microsoft.Extensions.Logging;

namespace CanonicalSeoAuditor.Services
{
    /// <summary>
    /// Self-contained service that fetches HTML directly from the web.
    /// This removes the dependency on the Node.js crawler terminal.
    /// </summary>
    public class CrawlerClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CrawlerClientService> _logger;

        public CrawlerClientService(HttpClient httpClient, ILogger<CrawlerClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Set a User-Agent so websites don't block the request as a bot
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) SEOAuditor/1.0");
        }

        /// <summary>
        /// Fetches the HTML content of a URL directly.
        /// </summary>
        public async Task<CrawlerResponse?> GetRenderedHtmlAsync(string targetUrl)
        {
            try
            {
                _logger.LogInformation("Directly fetching HTML for: {Url}", targetUrl);

                // Perform the GET request directly to the target website
                var response = await _httpClient.GetAsync(targetUrl);

                if (response.IsSuccessStatusCode)
                {
                    var htmlContent = await response.Content.ReadAsStringAsync();

                    return new CrawlerResponse
                    {
                        Url = targetUrl,
                        Content = htmlContent
                    };
                }

                _logger.LogError("Failed to fetch {Url}. Status Code: {StatusCode}", targetUrl, response.StatusCode);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while fetching {Url}", targetUrl);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unexpected error during direct crawl of {Url}", targetUrl);
                return null;
            }
        }
    }
}