using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SeoAuditor.Crawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SeoAuditor.Crawler.Services;

public interface ISeoAuditService
{
    Task<SeoAuditResult> PerformSeoAuditAsync(string url);
}

public class SeoAuditService : ISeoAuditService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SeoAuditService> _logger;

    public SeoAuditService(IHttpClientFactory httpClientFactory, ILogger<SeoAuditService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SeoAuditResult> PerformSeoAuditAsync(string url)
    {
        var result = new SeoAuditResult { Url = url };

        if (string.IsNullOrWhiteSpace(url))
        {
            result.ErrorMessage = "URL cannot be empty.";
            return result;
        }

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
            result.Url = url;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; SeoAuditorBot/1.0)");

            var startTime = DateTime.Now;
            var response = await client.GetAsync(url);
            var loadTime = (DateTime.Now - startTime).TotalSeconds;
            result.LoadTimeSeconds = Math.Round(loadTime, 2);

            if (!response.IsSuccessStatusCode)
            {
                result.ErrorMessage = $"Failed to crawl {url}. Status Code: {response.StatusCode}";
                return result;
            }

            // Security Checks from Headers/URL
            result.IsHttps = url.StartsWith("https", StringComparison.OrdinalIgnoreCase);
            result.HasHttps = result.IsHttps;
            result.HasHsts = response.Headers.Contains("Strict-Transport-Security");
            result.HasXContentTypeOptions = response.Headers.Contains("X-Content-Type-Options");
            result.HasXFrameOptions = response.Headers.Contains("X-Frame-Options");
            // Basic SSL check assumption: if HTTPS connection succeeded, cert is likely valid enough for client
            result.SslCertificateValid = result.IsHttps;

            var htmlContent = await response.Content.ReadAsStringAsync();
            result.EstimatedPageSizeKB = htmlContent.Length / 1024;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var rootNode = htmlDoc.DocumentNode;

            // --- Existing Checks ---

            // Title
            var titleNode = rootNode.SelectSingleNode("//title");
            result.Title = titleNode?.InnerText.Trim();
            result.TitleLength = result.Title?.Length ?? 0;
            result.TitleLengthValid = result.TitleLength >= 30 && result.TitleLength <= 60;

            // Meta Description
            var metaDescNode = rootNode.SelectSingleNode("//meta[@name='description']");
            result.MetaDescription = metaDescNode?.GetAttributeValue("content", "").Trim();
            result.MetaDescriptionLength = result.MetaDescription?.Length ?? 0;
            result.MetaDescriptionValid = result.MetaDescriptionLength >= 70 && result.MetaDescriptionLength <= 160;

            // H1 Tags
            var h1Nodes = rootNode.SelectNodes("//h1");
            if (h1Nodes != null)
            {
                result.H1Tags = h1Nodes.Select(n => n.InnerText.Trim()).ToList();
                result.H1Present = result.H1Tags.Any();
                result.MultipleH1 = result.H1Tags.Count > 1;
            }

            // H2 & H3 Tags
            var h2Nodes = rootNode.SelectNodes("//h2");
            if (h2Nodes != null) result.H2Tags = h2Nodes.Select(n => n.InnerText.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

            var h3Nodes = rootNode.SelectNodes("//h3");
            if (h3Nodes != null) result.H3Tags = h3Nodes.Select(n => n.InnerText.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

            // Images
            var imgNodes = rootNode.SelectNodes("//img");
            if (imgNodes != null)
            {
                result.ImageCount = imgNodes.Count;
                var imagesWithoutAlt = imgNodes.Where(n => string.IsNullOrWhiteSpace(n.GetAttributeValue("alt", ""))).ToList();
                result.ImagesWithoutAlt = imagesWithoutAlt.Count;
                result.ImagesWithoutAltUrls = imagesWithoutAlt.Select(n => n.GetAttributeValue("src", "unknown")).ToList();
            }

            // URL Length
            result.UrlLength = url.Length;
            result.UrlLengthValid = result.UrlLength < 75;

            // Canonical & Robots
            var canonicalNode = rootNode.SelectSingleNode("//link[@rel='canonical']");
            result.CanonicalLink = canonicalNode?.GetAttributeValue("href", "");
            result.CanonicalLinkPresent = !string.IsNullOrWhiteSpace(result.CanonicalLink);
            result.CanonicalTagMatchesUrl = result.CanonicalLinkPresent &&
                                            (result.CanonicalLink?.TrimEnd('/') == url.TrimEnd('/'));

            // Basic Robots Txt check (just existence in root, mocked for now as we don't crawl separate file yet)
            // Ideally this requires a separate HTTP request to /robots.txt
            result.RobotsTxtDetected = false; // Placeholder for separate check

            // Word Count (Basic approximation)
            var text = rootNode.InnerText;
            // Clean up scripts and styles from text count
            var scriptNodes = rootNode.SelectNodes("//script|//style");
            if (scriptNodes != null)
            {
                foreach (var script in scriptNodes) script.Remove();
            }
            var cleanText = rootNode.InnerText; // Get text after removing scripts
            result.WordCount = CountWords(cleanText);

            // --- New Checks (Links, Performance, Content, etc.) ---

            // Links Analysis
            var linkNodes = rootNode.SelectNodes("//a[@href]");
            if (linkNodes != null)
            {
                foreach (var link in linkNodes)
                {
                    var href = link.GetAttributeValue("href", "").Trim();
                    var rel = link.GetAttributeValue("rel", "").ToLower();
                    var anchorText = link.InnerText.Trim();

                    if (string.IsNullOrEmpty(anchorText) && !link.Descendants("img").Any())
                    {
                        result.LinksWithoutAnchorText++;
                    }

                    if (rel.Contains("nofollow"))
                    {
                        result.NoFollowLinksCount++;
                    }

                    if (href.StartsWith("/") || href.StartsWith(url) || !href.StartsWith("http"))
                    {
                        result.TotalInternalLinks++;
                    }
                    else
                    {
                        result.TotalExternalLinks++;
                    }
                }
            }

            result.InternalToExternalRatio = result.TotalExternalLinks > 0
                ? Math.Round((double)result.TotalInternalLinks / result.TotalExternalLinks, 2)
                : result.TotalInternalLinks;

            // Performance Resources
            var cssNodes = rootNode.SelectNodes("//link[@rel='stylesheet']");
            result.CssFilesCount = cssNodes?.Count ?? 0;

            var jsNodes = rootNode.SelectNodes("//script[@src]");
            result.JsFilesCount = jsNodes?.Count ?? 0;

            result.TotalResourcesCount = result.CssFilesCount + result.JsFilesCount + result.ImageCount;

            // Content Analysis
            result.KeywordDensity = CalculateKeywordDensity(cleanText);
            result.ReadingEaseScore = CalculateReadingEase(cleanText);

            // Check for Structured Data
            var jsonLdNodes = rootNode.SelectNodes("//script[@type='application/ld+json']");
            if (jsonLdNodes != null)
            {
                result.StructuredDataDetected.Add("JSON-LD");
            }
            if (rootNode.SelectNodes("//*[@itemtype]") != null)
            {
                result.StructuredDataDetected.Add("Microdata");
            }

            // Crawlability
            var metaRobots = rootNode.SelectSingleNode("//meta[@name='robots']");
            if (metaRobots != null)
            {
                var content = metaRobots.GetAttributeValue("content", "").ToLower();
                if (content.Contains("noindex")) result.MetaNoIndex = true;
            }

            // Calculate Overall Score and Generate Recommendations
            result.AuditSuccessful = true;

            // Generate Recommendations based on all gathered data
            result.Recommendations = GenerateRecommendations(result);

            // Recalculate Score based on new metrics
            result.OverallScore = CalculateEnhancedScore(result);

            return result;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing SEO audit for {Url}", url);
            result.ErrorMessage = $"An error occurred: {ex.Message}";
            result.AuditSuccessful = false;
            return result;
        }
    }

    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        // Simple regex to split by whitespace
        return Regex.Split(text.Trim(), @"\s+").Length;
    }

    private Dictionary<string, double> CalculateKeywordDensity(string text)
    {
        var density = new Dictionary<string, double>();
        if (string.IsNullOrWhiteSpace(text)) return density;

        var words = Regex.Split(text.ToLower(), @"\W+")
            .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length > 3) // Filter short words
            .ToList();

        if (words.Count == 0) return density;

        var groups = words.GroupBy(w => w)
                          .OrderByDescending(g => g.Count())
                          .Take(10); // Top 10 keywords

        foreach (var group in groups)
        {
            density[group.Key] = Math.Round((double)group.Count() / words.Count * 100, 2);
        }

        return density;
    }

    private double CalculateReadingEase(string text)
    {
        // Flesch-Kincaid Reading Ease (Approximation)
        // Formula: 206.835 - 1.015(total words/total sentences) - 84.6(total syllables/total words)

        if (string.IsNullOrWhiteSpace(text)) return 0;

        var sentences = Regex.Split(text, @"[.!?]+").Length;
        var words = CountWords(text);

        if (words == 0 || sentences == 0) return 0;

        // Syllable approximation: avg 1.5 per word for simplicity in this context
        // Real implementation requires dictionary or complex regex
        double avgSyllablesPerWord = 1.5;

        var score = 206.835 - (1.015 * (words / (double)sentences)) - (84.6 * avgSyllablesPerWord);
        return Math.Clamp(Math.Round(score, 1), 0, 100);
    }

    private int CalculateEnhancedScore(SeoAuditResult result)
    {
        int score = 100;

        // Deductions
        if (result.TitleLength < 10 || result.TitleLength > 70) score -= 10;
        if (string.IsNullOrEmpty(result.MetaDescription)) score -= 10;
        if (!result.H1Present) score -= 10;
        if (result.MultipleH1) score -= 5;
        if (result.ImagesWithoutAlt > 0) score -= 5;
        if (!result.IsHttps) score -= 10;
        if (result.LoadTimeSeconds > 3.0) score -= 10;
        if (result.WordCount < 300) score -= 10;
        if (result.BrokenLinksCount > 0) score -= 5;
        if (!result.CanonicalLinkPresent) score -= 5;

        return Math.Max(0, score);
    }

    private List<Recommendation> GenerateRecommendations(SeoAuditResult result)
    {
        var recommendations = new List<Recommendation>();

        // 1. Meta Tags
        if (string.IsNullOrEmpty(result.Title))
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.MetaTags,
                Priority = RecommendationPriority.Critical,
                Message = "Missing Title Tag",
                ActionableAdvice = "Add a descriptive <title> tag to the <head> section.",
                ImpactScore = 10
            });
        }
        else if (!result.TitleLengthValid)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.MetaTags,
                Priority = RecommendationPriority.Medium,
                Message = $"Title length is {result.TitleLength} characters (Recommended: 30-60)",
                ActionableAdvice = "Update page title to be between 30 and 60 characters.",
                ImpactScore = 5
            });
        }

        if (string.IsNullOrEmpty(result.MetaDescription))
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.MetaTags,
                Priority = RecommendationPriority.High,
                Message = "Missing Meta Description",
                ActionableAdvice = "Add a <meta name=\"description\"> tag summarizing the page content.",
                ImpactScore = 8
            });
        }

        // 2. Content
        if (!result.H1Present)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Content,
                Priority = RecommendationPriority.High,
                Message = "Missing H1 Heading",
                ActionableAdvice = "Ensure the page has exactly one <h1> tag describing the main topic.",
                ImpactScore = 10
            });
        }
        else if (result.MultipleH1)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Content,
                Priority = RecommendationPriority.Medium,
                Message = "Multiple H1 Tags Found",
                ActionableAdvice = "Use only one <h1> tag per page. Use <h2>-<h6> for subsections.",
                ImpactScore = 5
            });
        }

        if (result.ImagesWithoutAlt > 0)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Content,
                Priority = RecommendationPriority.High,
                Message = $"{result.ImagesWithoutAlt} Images Missing Alt Text",
                ActionableAdvice = "Add descriptive 'alt' attributes to all <img> tags for accessibility and SEO.",
                ImpactScore = 5
            });
        }

        if (result.WordCount < 300)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Content,
                Priority = RecommendationPriority.Medium,
                Message = "Low Word Count",
                ActionableAdvice = "Consider adding more substantial content (at least 300 words).",
                ImpactScore = 5
            });
        }

        // 3. Performance
        if (result.LoadTimeSeconds > 2.5)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Performance,
                Priority = RecommendationPriority.High,
                Message = $"Slow Page Load: {result.LoadTimeSeconds}s",
                ActionableAdvice = "Optimize images, minify CSS/JS, and use caching to reduce load time below 2.5s.",
                ImpactScore = 8
            });
        }

        if (result.TotalResourcesCount > 50)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Performance,
                Priority = RecommendationPriority.Medium,
                Message = $"High Request Count: {result.TotalResourcesCount}",
                ActionableAdvice = "Combine CSS/JS files and use sprites to reduce HTTP requests.",
                ImpactScore = 5
            });
        }

        // 4. Security
        if (!result.IsHttps)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Security,
                Priority = RecommendationPriority.Critical,
                Message = "Not Using HTTPS",
                ActionableAdvice = "Install an SSL certificate and redirect all traffic to HTTPS.",
                ImpactScore = 10
            });
        }

        // 5. Links
        if (result.LinksWithoutAnchorText > 0)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Links,
                Priority = RecommendationPriority.Medium,
                Message = $"{result.LinksWithoutAnchorText} Links Missing Anchor Text",
                ActionableAdvice = "Ensure all <a> tags have descriptive text inside them.",
                ImpactScore = 3
            });
        }

        // 6. Crawlability
        if (!result.CanonicalLinkPresent)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Crawlability,
                Priority = RecommendationPriority.Medium,
                Message = "Missing Canonical Tag",
                ActionableAdvice = "Add a <link rel=\"canonical\"> tag to prevent duplicate content issues.",
                ImpactScore = 5
            });
        }

        if (result.MetaNoIndex)
        {
            recommendations.Add(new Recommendation
            {
                Category = AuditCategory.Crawlability,
                Priority = RecommendationPriority.High,
                Message = "Page is NoIndexed",
                ActionableAdvice = "Remove 'noindex' from meta robots if you want this page to appear in search results.",
                ImpactScore = 10
            });
        }

        return recommendations.OrderByDescending(r => r.Priority).ThenByDescending(r => r.ImpactScore).ToList();
    }
}