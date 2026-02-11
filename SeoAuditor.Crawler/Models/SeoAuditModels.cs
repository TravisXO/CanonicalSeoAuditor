using System;
using System.Collections.Generic;

namespace SeoAuditor.Crawler.Models;

public class SeoAuditRequest
{
    public required string Url { get; set; }
}

public class SeoAuditResult
{
    public string? Url { get; set; }
    public int OverallScore { get; set; }

    public string? Title { get; set; }
    public int TitleLength { get; set; }
    public bool TitleLengthValid { get; set; }

    public string? MetaDescription { get; set; }
    public int MetaDescriptionLength { get; set; }
    public bool MetaDescriptionValid { get; set; }

    public List<string> H1Tags { get; set; } = new();
    public bool H1Present { get; set; }
    public bool MultipleH1 { get; set; }

    public List<string> H2Tags { get; set; } = new();
    public List<string> H3Tags { get; set; } = new();

    public int ImageCount { get; set; }
    public int ImagesWithoutAlt { get; set; }
    public List<string> ImagesWithoutAltUrls { get; set; } = new();

    public bool IsHttps { get; set; }
    public int UrlLength { get; set; }
    public bool UrlLengthValid { get; set; }

    public string? CanonicalLink { get; set; }
    public bool CanonicalLinkPresent { get; set; }
    public string? RobotsTxt { get; set; }
    public bool SitemapPresent { get; set; }

    public double LoadTimeSeconds { get; set; }
    public int WordCount { get; set; }

    public bool AuditSuccessful { get; set; }
    public string? ErrorMessage { get; set; }

    public int TotalInternalLinks { get; set; }
    public int TotalExternalLinks { get; set; }
    public int BrokenLinksCount { get; set; }
    public List<string> BrokenLinkUrls { get; set; } = new();
    public int NoFollowLinksCount { get; set; }
    public int LinksWithoutAnchorText { get; set; }
    public string LinksStatus { get; set; } = "Unknown";

    public double InternalToExternalRatio { get; set; }
    public string LinkBalanceStatus { get; set; } = "Unknown";

    public int TotalResourcesCount { get; set; }
    public int CssFilesCount { get; set; }
    public int JsFilesCount { get; set; }
    public int RenderBlockingResources { get; set; }
    public string PerformanceStatus { get; set; } = "Unknown";

    public long EstimatedPageSizeKB { get; set; }
    public string PageSizeStatus { get; set; } = "Unknown";

    public bool HasHttps { get; set; }
    public bool HasHsts { get; set; }
    public bool HasXContentTypeOptions { get; set; }
    public bool HasXFrameOptions { get; set; }
    public bool SslCertificateValid { get; set; }
    public int SecurityScore { get; set; }
    public string SecurityStatus { get; set; } = "Unknown";

    public Dictionary<string, double> KeywordDensity { get; set; } = new();
    public double ReadingEaseScore { get; set; }
    public double TextToCodeRatio { get; set; }
    public List<string> StructuredDataDetected { get; set; } = new();
    public string ContentQualityStatus { get; set; } = "Unknown";

    public bool RobotsTxtDetected { get; set; }
    public bool SitemapXmlDetected { get; set; }
    public bool MetaNoIndex { get; set; }
    public bool CanonicalTagMatchesUrl { get; set; }
    public string CrawlabilityStatus { get; set; } = "Unknown";

    public List<Recommendation> Recommendations { get; set; } = new();
}

public class Recommendation
{
    public AuditCategory Category { get; set; }
    public RecommendationPriority Priority { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ActionableAdvice { get; set; } = string.Empty;
    public int ImpactScore { get; set; }
}

public enum AuditCategory
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

public enum RecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}