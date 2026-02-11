using System.Text.Json.Serialization;

namespace CanonicalSeoAuditor.Models
{
    /// <summary>
    /// The request object sent TO the Node.js crawler.
    /// Matches the 'body' validation in server.js.
    /// </summary>
    public class CrawlerRequest
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// The successful response received FROM the Node.js crawler.
    /// Matches the 'res.json' structure in server.js.
    /// </summary>
    public class CrawlerResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Captures error details if the crawler service fails.
    /// </summary>
    public class CrawlerErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        public string Details { get; set; } = string.Empty;
    }
}