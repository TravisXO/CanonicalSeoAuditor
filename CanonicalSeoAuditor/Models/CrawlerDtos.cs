using System.Text.Json.Serialization;

namespace CanonicalSeoAuditor.Models;

public class SeoAuditRequestDto
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}

public class SeoAuditResultDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("overallScore")]
    public int OverallScore { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("titleLength")]
    public int TitleLength { get; set; }

    [JsonPropertyName("titleLengthValid")]
    public bool TitleLengthValid { get; set; }

    [JsonPropertyName("metaDescription")]
    public string? MetaDescription { get; set; }

    [JsonPropertyName("metaDescriptionLength")]
    public int MetaDescriptionLength { get; set; }

    [JsonPropertyName("metaDescriptionValid")]
    public bool MetaDescriptionValid { get; set; }

    [JsonPropertyName("h1Tags")]
    public List<string> H1Tags { get; set; } = new();

    [JsonPropertyName("h1Present")]
    public bool H1Present { get; set; }

    [JsonPropertyName("multipleH1")]
    public bool MultipleH1 { get; set; }

    [JsonPropertyName("h2Tags")]
    public List<string> H2Tags { get; set; } = new();

    [JsonPropertyName("h3Tags")]
    public List<string> H3Tags { get; set; } = new();

    [JsonPropertyName("imageCount")]
    public int ImageCount { get; set; }

    [JsonPropertyName("imagesWithoutAlt")]
    public int ImagesWithoutAlt { get; set; }

    [JsonPropertyName("imagesWithoutAltUrls")]
    public List<string> ImagesWithoutAltUrls { get; set; } = new();

    [JsonPropertyName("isHttps")]
    public bool IsHttps { get; set; }

    [JsonPropertyName("urlLength")]
    public int UrlLength { get; set; }

    [JsonPropertyName("urlLengthValid")]
    public bool UrlLengthValid { get; set; }

    [JsonPropertyName("canonicalLink")]
    public string? CanonicalLink { get; set; }

    [JsonPropertyName("canonicalLinkPresent")]
    public bool CanonicalLinkPresent { get; set; }

    [JsonPropertyName("robotsTxt")]
    public string? RobotsTxt { get; set; }

    [JsonPropertyName("sitemapPresent")]
    public bool SitemapPresent { get; set; }

    [JsonPropertyName("loadTimeSeconds")]
    public double LoadTimeSeconds { get; set; }

    [JsonPropertyName("wordCount")]
    public int WordCount { get; set; }

    [JsonPropertyName("auditSuccessful")]
    public bool AuditSuccessful { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("totalInternalLinks")]
    public int TotalInternalLinks { get; set; }

    [JsonPropertyName("totalExternalLinks")]
    public int TotalExternalLinks { get; set; }

    [JsonPropertyName("brokenLinksCount")]
    public int BrokenLinksCount { get; set; }

    [JsonPropertyName("brokenLinkUrls")]
    public List<string> BrokenLinkUrls { get; set; } = new();

    [JsonPropertyName("noFollowLinksCount")]
    public int NoFollowLinksCount { get; set; }

    [JsonPropertyName("linksWithoutAnchorText")]
    public int LinksWithoutAnchorText { get; set; }

    [JsonPropertyName("linksStatus")]
    public string LinksStatus { get; set; } = "Unknown";

    [JsonPropertyName("internalToExternalRatio")]
    public double InternalToExternalRatio { get; set; }

    [JsonPropertyName("linkBalanceStatus")]
    public string LinkBalanceStatus { get; set; } = "Unknown";

    [JsonPropertyName("totalResourcesCount")]
    public int TotalResourcesCount { get; set; }

    [JsonPropertyName("cssFilesCount")]
    public int CssFilesCount { get; set; }

    [JsonPropertyName("jsFilesCount")]
    public int JsFilesCount { get; set; }

    [JsonPropertyName("renderBlockingResources")]
    public int RenderBlockingResources { get; set; }

    [JsonPropertyName("performanceStatus")]
    public string PerformanceStatus { get; set; } = "Unknown";

    [JsonPropertyName("estimatedPageSizeKB")]
    public long EstimatedPageSizeKB { get; set; }

    [JsonPropertyName("pageSizeStatus")]
    public string PageSizeStatus { get; set; } = "Unknown";

    [JsonPropertyName("hasHttps")]
    public bool HasHttps { get; set; }

    [JsonPropertyName("hasHsts")]
    public bool HasHsts { get; set; }

    [JsonPropertyName("hasXContentTypeOptions")]
    public bool HasXContentTypeOptions { get; set; }

    [JsonPropertyName("hasXFrameOptions")]
    public bool HasXFrameOptions { get; set; }

    [JsonPropertyName("sslCertificateValid")]
    public bool SslCertificateValid { get; set; }

    [JsonPropertyName("securityScore")]
    public int SecurityScore { get; set; }

    [JsonPropertyName("securityStatus")]
    public string SecurityStatus { get; set; } = "Unknown";

    [JsonPropertyName("keywordDensity")]
    public Dictionary<string, double> KeywordDensity { get; set; } = new();

    [JsonPropertyName("readingEaseScore")]
    public double ReadingEaseScore { get; set; }

    [JsonPropertyName("textToCodeRatio")]
    public double TextToCodeRatio { get; set; }

    [JsonPropertyName("structuredDataDetected")]
    public List<string> StructuredDataDetected { get; set; } = new();

    [JsonPropertyName("contentQualityStatus")]
    public string ContentQualityStatus { get; set; } = "Unknown";

    [JsonPropertyName("robotsTxtDetected")]
    public bool RobotsTxtDetected { get; set; }

    [JsonPropertyName("sitemapXmlDetected")]
    public bool SitemapXmlDetected { get; set; }

    [JsonPropertyName("metaNoIndex")]
    public bool MetaNoIndex { get; set; }

    [JsonPropertyName("canonicalTagMatchesUrl")]
    public bool CanonicalTagMatchesUrl { get; set; }

    [JsonPropertyName("crawlabilityStatus")]
    public string CrawlabilityStatus { get; set; } = "Unknown";

    [JsonPropertyName("recommendations")]
    public List<RecommendationDto> Recommendations { get; set; } = new();
}

public class RecommendationDto
{
    [JsonPropertyName("category")]
    public AuditCategoryDto Category { get; set; }

    [JsonPropertyName("priority")]
    public RecommendationPriorityDto Priority { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("actionableAdvice")]
    public string ActionableAdvice { get; set; } = string.Empty;

    [JsonPropertyName("impactScore")]
    public int ImpactScore { get; set; }
}

public enum AuditCategoryDto
{
    MetaTags,
    Content,
    Performance,
    Security,
    Links,
    Crawlability,
    Mobile,
    Advanced
}

public enum RecommendationPriorityDto
{
    Low,
    Medium,
    High,
    Critical
}