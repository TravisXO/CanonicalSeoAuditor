using System;
using System.Collections.Generic;
using System.Linq;

namespace SeoAuditor.Crawler.Models
{
    /// <summary>
    /// Represents the expanded report of an SEO audit.
    /// Stores metadata, content structure, technical SEO, social graph, media, and advanced quality metrics.
    /// </summary>
    public class SeoAuditResult
    {
        public string Url { get; set; } = string.Empty;
        public DateTime AuditDate { get; set; } = DateTime.Now;

        // ==================================================================================
        // SCORING (New)
        // ==================================================================================
        public int OverallScore { get; set; }
        public string Grade => OverallScore >= 90 ? "Excellent" :
                               OverallScore >= 70 ? "Good" :
                               OverallScore >= 50 ? "Fair" : "Poor";

        // Category Sub-Scores (0-100)
        public int MetadataScore { get; set; }
        public int ContentScore { get; set; }
        public int TechnicalScore { get; set; }
        public int SocialScore { get; set; }
        public int AccessibilityScore { get; set; }

        // ==================================================================================
        // CATEGORY 1: METADATA
        // ==================================================================================
        public string? MetaTitle { get; set; }
        public int MetaTitleLength => MetaTitle?.Length ?? 0;
        public string MetaTitleStatus { get; set; } = "Unknown";

        public string? MetaDescription { get; set; }
        public int MetaDescriptionLength => MetaDescription?.Length ?? 0;
        public string MetaDescriptionStatus { get; set; } = "Unknown";

        public string? MetaRobots { get; set; }
        public string MetaRobotsStatus { get; set; } = "Unknown";

        public string? MetaViewport { get; set; }
        public string MetaViewportStatus { get; set; } = "Unknown";

        public bool HasMetaKeywords { get; set; }
        public string MetaKeywordsStatus => HasMetaKeywords ? "Warning" : "Good";

        public string? Charset { get; set; }
        public string CharsetStatus { get; set; } = "Unknown";

        public string? Language { get; set; }
        public string LanguageStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 2: CONTENT STRUCTURE
        // ==================================================================================
        public int H1Count { get; set; }
        public List<string> H1Contents { get; set; } = new();
        public string H1Status { get; set; } = "Unknown";

        public Dictionary<string, int> HeadingCounts { get; set; } = new();
        public List<string> HeadingIssues { get; set; } = new();
        public string HeadingStructureStatus => HeadingIssues.Any() ? "Warning" : "Good";

        public int WordCount { get; set; }
        public string WordCountStatus { get; set; } = "Unknown";

        public int ParagraphCount { get; set; }
        public double TextInParagraphsPercentage { get; set; }
        public string ParagraphStatus { get; set; } = "Unknown";

        public int ListCount { get; set; }
        public int OrphanListItems { get; set; }
        public string ListStatus { get; set; } = "Unknown";

        public List<string> SchemaTypes { get; set; } = new();
        public string SchemaStatus => SchemaTypes.Any() ? "Good" : "Info";

        public int TotalHtmlSize { get; set; }
        public int TextContentSize { get; set; }
        public double TextToHtmlRatio { get; set; }
        public string RatioStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 3 & 6: IMAGES
        // ==================================================================================
        public int TotalImages { get; set; }
        public int ImagesMissingAlt { get; set; }
        public List<string> ImageSourcesMissingAlt { get; set; } = new();
        public string ImageAltStatus { get; set; } = "Unknown";

        public int ImagesWithGenericNames { get; set; }
        public string ImageFilenameStatus { get; set; } = "Unknown";

        public int ImagesMissingDimensions { get; set; }
        public string ImageDimensionStatus { get; set; } = "Unknown";

        public int ImagesLazyLoaded { get; set; }
        public string ImageLazyLoadStatus { get; set; } = "Unknown";

        public int ImagesModernFormat { get; set; }
        public string ImageFormatStatus { get; set; } = "Unknown";

        public int ImagesResponsive { get; set; }
        public string ImageResponsiveStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 4: SOCIAL / OPEN GRAPH
        // ==================================================================================
        public string? OgTitle { get; set; }
        public int OgTitleLength => OgTitle?.Length ?? 0;
        public string OgTitleStatus { get; set; } = "Unknown";

        public string? OgDescription { get; set; }
        public int OgDescriptionLength => OgDescription?.Length ?? 0;
        public string OgDescriptionStatus { get; set; } = "Unknown";

        public string? OgImage { get; set; }
        public string OgImageStatus { get; set; } = "Unknown";

        public string? OgType { get; set; }
        public string OgTypeStatus { get; set; } = "Unknown";

        public string? OgUrl { get; set; }
        public string OgUrlStatus { get; set; } = "Unknown";

        public bool HasTwitterCard { get; set; }
        public bool HasTwitterImage { get; set; }
        public string TwitterCardType { get; set; } = "None";
        public string TwitterStatus { get; set; } = "Unknown";

        public string? FbAppId { get; set; }
        public string? ArticlePublisher { get; set; }
        public string FacebookStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 5: TECHNICAL SEO
        // ==================================================================================
        public string? FoundCanonical { get; set; }
        public bool IsCanonicalCorrect { get; set; }
        public string CanonicalStatus { get; set; } = "Unknown";

        public int HreflangCount { get; set; }
        public bool HasXDefault { get; set; }
        public string HreflangStatus { get; set; } = "Unknown";

        public string? FaviconUrl { get; set; }
        public string FaviconStatus { get; set; } = "Unknown";

        public int PreconnectCount { get; set; }
        public string PreconnectStatus { get; set; } = "Unknown";

        public string? MobileAlternate { get; set; }
        public string MobileAlternateStatus { get; set; } = "Unknown";

        public string? AmpUrl { get; set; }
        public string AmpStatus { get; set; } = "Unknown";

        public string? RssFeedUrl { get; set; }
        public string RssStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 7: MEDIA
        // ==================================================================================
        public int VideoCount { get; set; }
        public List<string> VideoIssues { get; set; } = new();
        public string VideoStatus { get; set; } = "Unknown";

        public int AudioCount { get; set; }
        public List<string> AudioIssues { get; set; } = new();
        public string AudioStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 8: FORMS
        // ==================================================================================
        public int FormCount { get; set; }
        public List<string> FormIssues { get; set; } = new();
        public string FormStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 9: DEPRECATIONS & ISSUES
        // ==================================================================================
        public int DeprecatedTagsCount { get; set; }
        public int InlineStylesCount { get; set; }
        public int DomNodeCount { get; set; }
        public bool HasFlash { get; set; }
        public int IframeCount { get; set; }
        public string DeprecationStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 10: ACCESSIBILITY (SEO Impact)
        // ==================================================================================
        public bool HasMainLandmark { get; set; }
        public bool HasNavLandmark { get; set; }
        public bool HasFooterLandmark { get; set; }
        public bool HasSkipLink { get; set; }
        public int TabIndexIssues { get; set; }
        public string AccessibilityStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 11: CONTENT QUALITY SIGNALS
        // ==================================================================================
        public double AverageSentenceLength { get; set; }
        public double FleschReadingEase { get; set; }
        public string? PublishedDate { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Author { get; set; }
        public string ContentQualityStatus { get; set; } = "Unknown";

        // ==================================================================================
        // CATEGORY 12: SCHEMA/STRUCTURED DATA (Enhanced)
        // ==================================================================================
        public Dictionary<string, string> SchemaDetails { get; set; } = new();
        public string StructuredDataStatus { get; set; } = "Unknown";
    }
}