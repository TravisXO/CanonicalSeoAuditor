using System;
using System.Collections.Generic;

namespace SeoAuditor.Crawler.Models;

public class SeoAuditRequest
{
    public required string Url { get; set; }
}

public class SeoAuditResult
{
    // --- Core Identity ---
    public string? Url { get; set; }
    public DateTime AuditDate { get; set; } = DateTime.Now;
    public int OverallScore { get; set; }
    public string Grade { get; set; } = "F";

    // --- Score Categories (View Compatibility) ---
    public int MetadataScore { get; set; }
    public int ContentScore { get; set; }
    public int TechnicalScore { get; set; }
    public int SocialScore { get; set; }
    public int AccessibilityScore { get; set; }

    // --- Metadata Section ---
    public string? Title { get; set; } // Crawler logic uses this
    public string? MetaTitle { get; set; } // View uses this
    public int TitleLength { get; set; }
    public int MetaTitleLength { get; set; } // View uses this
    public bool TitleLengthValid { get; set; }
    public string MetaTitleStatus { get; set; } = "Unknown";

    public string? MetaDescription { get; set; }
    public int MetaDescriptionLength { get; set; }
    public bool MetaDescriptionValid { get; set; }
    public string MetaDescriptionStatus { get; set; } = "Unknown";

    public string MetaRobots { get; set; } = "None";
    public string MetaRobotsStatus { get; set; } = "Unknown";

    public string MetaViewportStatus { get; set; } = "Unknown";

    public string Charset { get; set; } = "UTF-8";
    public string CharsetStatus { get; set; } = "Unknown";

    public bool HasMetaKeywords { get; set; }

    // --- Content Section ---
    public int WordCount { get; set; }
    public string WordCountStatus { get; set; } = "Unknown";

    public List<string> H1Tags { get; set; } = new();
    public List<string> H1Contents { get; set; } = new(); // View alias
    public bool H1Present { get; set; }
    public bool MultipleH1 { get; set; }
    public int H1Count { get; set; }
    public string H1Status { get; set; } = "Unknown";

    public List<string> H2Tags { get; set; } = new();
    public List<string> H3Tags { get; set; } = new();

    public double TextToCodeRatio { get; set; }
    public double TextToHtmlRatio { get; set; } // View alias
    public string RatioStatus { get; set; } = "Unknown";

    public string HeadingStructureStatus { get; set; } = "Unknown";
    public List<string> HeadingIssues { get; set; } = new();

    public int ParagraphCount { get; set; }
    public string ParagraphStatus { get; set; } = "Unknown";

    public int OrphanListItems { get; set; }
    public string ListStatus { get; set; } = "Unknown";

    // --- Images Section ---
    public int ImageCount { get; set; }
    public int TotalImages { get; set; } // View alias

    public int ImagesWithoutAlt { get; set; }
    public int ImagesMissingAlt { get; set; } // View alias
    public List<string> ImagesWithoutAltUrls { get; set; } = new();
    public string ImageAltStatus { get; set; } = "Unknown";

    public int ImagesMissingDimensions { get; set; }
    public string ImageDimensionStatus { get; set; } = "Unknown";

    public int ImagesModernFormat { get; set; }
    public string ImageFormatStatus { get; set; } = "Unknown";

    public int ImagesLazyLoaded { get; set; }
    public string ImageLazyLoadStatus { get; set; } = "Unknown";

    public int ImagesWithGenericNames { get; set; }
    public string ImageFilenameStatus { get; set; } = "Unknown";

    // --- Social Media (View Requirements) ---
    public string? OgTitle { get; set; }
    public string OgTitleStatus { get; set; } = "Unknown";

    public string? OgDescription { get; set; }
    public string OgDescriptionStatus { get; set; } = "Unknown";

    public string? OgImage { get; set; }
    public string OgImageStatus { get; set; } = "Unknown";

    public string TwitterCardType { get; set; } = "None";
    public string TwitterStatus { get; set; } = "Unknown";

    public string? FbAppId { get; set; }
    public string FacebookStatus { get; set; } = "Unknown";

    // --- Technical SEO ---
    public bool IsHttps { get; set; }
    public int UrlLength { get; set; }
    public bool UrlLengthValid { get; set; }

    public string? CanonicalLink { get; set; }
    public string? FoundCanonical { get; set; } // View alias
    public bool CanonicalLinkPresent { get; set; }
    public bool CanonicalTagMatchesUrl { get; set; }
    public string CanonicalStatus { get; set; } = "Unknown";

    public string? RobotsTxt { get; set; }
    public bool RobotsTxtDetected { get; set; }
    public bool MetaNoIndex { get; set; }
    public string CrawlabilityStatus { get; set; } = "Unknown";

    public bool SitemapPresent { get; set; }
    public bool SitemapXmlDetected { get; set; } // View alias

    public int HreflangCount { get; set; }
    public bool HasXDefault { get; set; }
    public string HreflangStatus { get; set; } = "Unknown";

    public string? FaviconUrl { get; set; }
    public string FaviconStatus { get; set; } = "Unknown";

    public string PreconnectStatus { get; set; } = "Unknown";
    public string AmpStatus { get; set; } = "Unknown";
    public string RssStatus { get; set; } = "Unknown";

    // --- Performance (New & Legacy) ---
    public double LoadTimeSeconds { get; set; }

    public int TotalResourcesCount { get; set; }
    public int CssFilesCount { get; set; }
    public int JsFilesCount { get; set; }
    public int RenderBlockingResources { get; set; }
    public string PerformanceStatus { get; set; } = "Unknown";

    public long EstimatedPageSizeKB { get; set; }
    public string PageSizeStatus { get; set; } = "Unknown";

    // --- Security (New) ---
    public bool HasHttps { get; set; }
    public bool HasHsts { get; set; }
    public bool HasXContentTypeOptions { get; set; }
    public bool HasXFrameOptions { get; set; }
    public bool SslCertificateValid { get; set; }
    public int SecurityScore { get; set; }
    public string SecurityStatus { get; set; } = "Unknown";

    // --- Links (New & Legacy) ---
    public int TotalInternalLinks { get; set; }
    public int TotalExternalLinks { get; set; }
    public int BrokenLinksCount { get; set; }
    public List<string> BrokenLinkUrls { get; set; } = new();
    public int NoFollowLinksCount { get; set; }
    public int LinksWithoutAnchorText { get; set; }
    public string LinksStatus { get; set; } = "Unknown";
    public double InternalToExternalRatio { get; set; }
    public string LinkBalanceStatus { get; set; } = "Unknown";

    // --- Content Quality & E-E-A-T ---
    public Dictionary<string, double> KeywordDensity { get; set; } = new();

    public double ReadingEaseScore { get; set; }
    public double FleschReadingEase { get; set; } // View alias
    public double AverageSentenceLength { get; set; }

    public string? Author { get; set; }
    public string? PublishedDate { get; set; }
    public string? ModifiedDate { get; set; }

    public List<string> StructuredDataDetected { get; set; } = new();
    public List<string> SchemaTypes { get; set; } = new(); // View alias
    public Dictionary<string, string> SchemaDetails { get; set; } = new();
    public string StructuredDataStatus { get; set; } = "Unknown";
    public string ContentQualityStatus { get; set; } = "Unknown";

    // --- Media (Audio/Video) ---
    public int VideoCount { get; set; }
    public List<string> VideoIssues { get; set; } = new();
    public string VideoStatus { get; set; } = "Unknown";

    public int AudioCount { get; set; }
    public List<string> AudioIssues { get; set; } = new();
    public string AudioStatus { get; set; } = "Unknown";

    // --- Forms ---
    public int FormCount { get; set; }
    public List<string> FormIssues { get; set; } = new();
    public string FormStatus { get; set; } = "Unknown";

    // --- Code Quality / Deprecations ---
    public int DeprecatedTagsCount { get; set; }
    public int InlineStylesCount { get; set; }
    public bool HasFlash { get; set; }
    public int DomNodeCount { get; set; }

    // --- Accessibility ---
    public bool HasMainLandmark { get; set; }
    public bool HasNavLandmark { get; set; }
    public bool HasFooterLandmark { get; set; }
    public bool HasSkipLink { get; set; }
    public int TabIndexIssues { get; set; }

    // --- Status ---
    public bool AuditSuccessful { get; set; }
    public string? ErrorMessage { get; set; }

    // --- New Recommendation Engine ---
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