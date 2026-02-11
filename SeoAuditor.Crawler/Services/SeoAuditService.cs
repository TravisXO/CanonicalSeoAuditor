using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SeoAuditor.Crawler.Models;

namespace SeoAuditor.Crawler.Services
{
    public class SeoAuditService
    {
        private readonly ILogger<SeoAuditService> _logger;

        public SeoAuditService(ILogger<SeoAuditService> logger)
        {
            _logger = logger;
        }

        public SeoAuditResult AuditHtml(string url, string html)
        {
            var result = new SeoAuditResult { Url = url };
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            _logger.LogInformation("Starting Full SEO analysis (Cat 1-12) for: {Url}", url);

            // ==================================================================================
            // CATEGORY 1: METADATA
            // ==================================================================================
            // 1.1 Meta Title
            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            var allTitleNodes = doc.DocumentNode.SelectNodes("//title");

            if (titleNode == null) result.MetaTitleStatus = "Critical (Missing)";
            else
            {
                result.MetaTitle = titleNode.InnerText.Trim();
                if (string.IsNullOrWhiteSpace(result.MetaTitle)) result.MetaTitleStatus = "Critical (Empty)";
                else if (allTitleNodes.Count > 1) result.MetaTitleStatus = "Warning (Multiple Titles)";
                else if (result.MetaTitle.Contains("Untitled", StringComparison.OrdinalIgnoreCase)) result.MetaTitleStatus = "Warning (Placeholder Text)";
                else if (result.MetaTitleLength < 30) result.MetaTitleStatus = "Warning (Too Short)";
                else if (result.MetaTitleLength > 60) result.MetaTitleStatus = "Warning (Too Long)";
                else result.MetaTitleStatus = "Good";
            }

            // 1.2 Meta Description
            var descNode = doc.DocumentNode.SelectSingleNode("//meta[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='description']");
            if (descNode == null) result.MetaDescriptionStatus = "Critical (Missing)";
            else
            {
                result.MetaDescription = descNode.GetAttributeValue("content", "").Trim();
                if (string.IsNullOrWhiteSpace(result.MetaDescription)) result.MetaDescriptionStatus = "Critical (Empty)";
                else if (result.MetaDescriptionLength < 70) result.MetaDescriptionStatus = "Warning (Too Short)";
                else if (result.MetaDescriptionLength > 160) result.MetaDescriptionStatus = "Warning (Too Long)";
                else result.MetaDescriptionStatus = "Good";
            }

            // 1.3 - 1.7 Helpers
            CheckMetaHelpers(doc, result);


            // ==================================================================================
            // CATEGORY 2: CONTENT STRUCTURE
            // ==================================================================================
            // 2.1 H1
            var h1Nodes = doc.DocumentNode.SelectNodes("//h1");
            result.H1Count = h1Nodes?.Count ?? 0;
            if (result.H1Count == 0) result.H1Status = "Critical (Missing)";
            else if (result.H1Count > 1)
            {
                result.H1Status = "Critical (Multiple H1s)";
                result.H1Contents = h1Nodes!.Select(n => n.InnerText.Trim()).ToList();
            }
            else
            {
                var h1 = h1Nodes![0].InnerText.Trim();
                result.H1Contents.Add(h1);
                if (h1.Length < 20 || h1.Length > 70) result.H1Status = "Warning (Length Issue)";
                else result.H1Status = "Good";
            }

            // 2.2 Headings
            var headings = doc.DocumentNode.SelectNodes("//h2|//h3|//h4|//h5|//h6");
            if (headings != null)
            {
                int prevLevel = 1;
                foreach (var h in headings)
                {
                    int level = int.Parse(h.Name.Substring(1));
                    string tag = h.Name.ToUpper();
                    if (!result.HeadingCounts.ContainsKey(tag)) result.HeadingCounts[tag] = 0;
                    result.HeadingCounts[tag]++;
                    if (level > prevLevel + 1) result.HeadingIssues.Add($"Skipped level: H{prevLevel} -> {tag}");
                    prevLevel = level;
                }
            }

            // 2.5 Lists
            var lists = doc.DocumentNode.SelectNodes("//ul|//ol");
            result.ListCount = lists?.Count ?? 0;
            var lis = doc.DocumentNode.SelectNodes("//li");
            if (lis != null)
            {
                foreach (var li in lis) if (li.ParentNode.Name != "ul" && li.ParentNode.Name != "ol") result.OrphanListItems++;
            }
            result.ListStatus = result.OrphanListItems > 0 ? "Warning (Orphan Items)" : "Good";

            // ==================================================================================
            // CATEGORY 4 & 5 & 7: TECHNICAL / SOCIAL / MEDIA
            // ==================================================================================
            CheckSocialAndTechnical(url, doc, result);
            CheckMedia(doc, result);


            // ==================================================================================
            // CATEGORY 6: IMAGES (Enhanced)
            // ==================================================================================
            var imgNodes = doc.DocumentNode.SelectNodes("//img");
            if (imgNodes != null)
            {
                result.TotalImages = imgNodes.Count;
                foreach (var img in imgNodes)
                {
                    string alt = img.GetAttributeValue("alt", "");
                    string src = img.GetAttributeValue("src", "unknown").ToLower();

                    if (string.IsNullOrWhiteSpace(alt)) { result.ImagesMissingAlt++; result.ImageSourcesMissingAlt.Add(src); }

                    string filename = Path.GetFileName(src);
                    if (filename.Contains("_") || Regex.IsMatch(filename, @"^img\d+")) result.ImagesWithGenericNames++;

                    if (!img.Attributes.Contains("width") || !img.Attributes.Contains("height")) result.ImagesMissingDimensions++;
                    if (img.GetAttributeValue("loading", "") == "lazy") result.ImagesLazyLoaded++;

                    string ext = Path.GetExtension(src);
                    if (ext == ".webp" || ext == ".avif") result.ImagesModernFormat++;

                    if (img.Attributes.Contains("srcset") || img.ParentNode.Name == "picture") result.ImagesResponsive++;
                }

                result.ImageAltStatus = result.ImagesMissingAlt == 0 ? "Good" : "Warning";
                result.ImageDimensionStatus = result.ImagesMissingDimensions == 0 ? "Good" : "Critical";
            }
            else
            {
                result.ImageAltStatus = "Info";
                result.ImageDimensionStatus = "Info";
            }
            // Add default statuses for image metrics if no images, to avoid nulls
            result.ImageFilenameStatus = result.ImagesWithGenericNames == 0 ? "Good" : "Warning";
            result.ImageLazyLoadStatus = result.ImagesLazyLoaded > 0 ? "Good" : "Warning";
            result.ImageFormatStatus = result.ImagesModernFormat > 0 ? "Good" : "Warning";
            result.ImageResponsiveStatus = result.ImagesResponsive > 0 ? "Good" : "Warning";
            if (result.TotalImages == 0)
            {
                result.ImageFilenameStatus = "Info";
                result.ImageLazyLoadStatus = "Info";
                result.ImageFormatStatus = "Info";
                result.ImageResponsiveStatus = "Info";
            }


            // ==================================================================================
            // CATEGORY 8: FORMS
            // ==================================================================================
            var forms = doc.DocumentNode.SelectNodes("//form");
            result.FormCount = forms?.Count ?? 0;
            if (forms != null)
            {
                foreach (var f in forms)
                {
                    if (!f.Attributes.Contains("action")) result.FormIssues.Add("Form missing action attribute");
                    var submit = f.SelectSingleNode(".//input[@type='submit']|.//button[@type='submit']");
                    if (submit == null) result.FormIssues.Add("Form missing submit button");
                }
            }
            result.FormStatus = result.FormIssues.Any() ? "Warning" : "Good";

            // ==================================================================================
            // CATEGORY 9: DEPRECATIONS & ISSUES
            // ==================================================================================
            var deprecatedTags = doc.DocumentNode.SelectNodes("//center|//font|//marquee|//blink|//frame|//frameset|//big|//strike|//tt");
            result.DeprecatedTagsCount = deprecatedTags?.Count ?? 0;

            var inlineStyles = doc.DocumentNode.SelectNodes("//*[@style]");
            result.InlineStylesCount = inlineStyles?.Count ?? 0;

            result.DomNodeCount = doc.DocumentNode.SelectNodes("//*")?.Count ?? 0;

            var flash = doc.DocumentNode.SelectNodes("//embed[contains(@type, 'flash')]|//object[contains(@type, 'flash')]");
            result.HasFlash = flash != null;

            var iframes = doc.DocumentNode.SelectNodes("//iframe");
            result.IframeCount = iframes?.Count ?? 0;

            if (result.DeprecatedTagsCount > 0 || result.HasFlash) result.DeprecationStatus = "Critical";
            else if (result.InlineStylesCount > 10 || result.DomNodeCount > 1500) result.DeprecationStatus = "Warning";
            else result.DeprecationStatus = "Good";

            // ==================================================================================
            // CATEGORY 10: ACCESSIBILITY (SEO Impact)
            // ==================================================================================
            result.HasMainLandmark = doc.DocumentNode.SelectSingleNode("//main|//*[@role='main']") != null;
            result.HasNavLandmark = doc.DocumentNode.SelectSingleNode("//nav|//*[@role='navigation']") != null;
            result.HasFooterLandmark = doc.DocumentNode.SelectSingleNode("//footer|//*[@role='contentinfo']") != null;

            var skipLink = doc.DocumentNode.SelectSingleNode("//body//a[position() < 5 and contains(@href, '#')]");
            result.HasSkipLink = skipLink != null;

            var badTabIndex = doc.DocumentNode.SelectNodes("//*[@tabindex > 0]");
            result.TabIndexIssues = badTabIndex?.Count ?? 0;

            if (!result.HasMainLandmark || result.TabIndexIssues > 0) result.AccessibilityStatus = "Warning";
            else result.AccessibilityStatus = "Good";

            // ==================================================================================
            // CATEGORY 11: CONTENT QUALITY
            // ==================================================================================
            var pubDate = doc.DocumentNode.SelectSingleNode("//meta[@property='article:published_time']")?.GetAttributeValue("content", "");
            result.PublishedDate = !string.IsNullOrEmpty(pubDate) ? DateTime.TryParse(pubDate, out var d) ? d.ToShortDateString() : pubDate : "Not found";

            var modDate = doc.DocumentNode.SelectSingleNode("//meta[@property='article:modified_time']")?.GetAttributeValue("content", "");
            result.ModifiedDate = !string.IsNullOrEmpty(modDate) ? DateTime.TryParse(modDate, out var d2) ? d2.ToShortDateString() : modDate : "Not found";

            result.Author = doc.DocumentNode.SelectSingleNode("//meta[@name='author']|//meta[@property='article:author']")?.GetAttributeValue("content", "Not found");

            AnalyzeTextQuality(doc, result, html.Length);

            // ==================================================================================
            // CATEGORY 12: SCHEMA (Enhanced)
            // ==================================================================================
            var scriptTags = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
            result.SchemaTypes.Clear();

            if (scriptTags != null)
            {
                foreach (var script in scriptTags)
                {
                    string json = script.InnerText;
                    try
                    {
                        using (JsonDocument jDoc = JsonDocument.Parse(json))
                        {
                            var root = jDoc.RootElement;
                            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("@type", out var typeProp))
                            {
                                string type = typeProp.ToString();
                                result.SchemaTypes.Add(type);

                                if (type.Contains("BreadcrumbList")) result.SchemaDetails["Breadcrumb"] = "Present";
                                if (type.Contains("Article") || type.Contains("BlogPosting")) result.SchemaDetails["Article"] = "Present";
                                if (type.Contains("Product")) result.SchemaDetails["Product"] = "Present";
                                if (type.Contains("Organization")) result.SchemaDetails["Organization"] = "Present";
                                if (type.Contains("LocalBusiness")) result.SchemaDetails["LocalBusiness"] = "Present";
                                if (type.Contains("VideoObject")) result.SchemaDetails["Video"] = "Present";
                                if (type.Contains("FAQPage")) result.SchemaDetails["FAQ"] = "Present";
                            }
                        }
                    }
                    catch { result.SchemaTypes.Add("Invalid JSON-LD"); }
                }
            }
            result.StructuredDataStatus = result.SchemaTypes.Any() ? "Good" : "Info";

            // ==================================================================================
            // CALCULATE SCORES
            // ==================================================================================
            CalculateScores(result);

            return result;
        }

        // --- SCORING LOGIC ---
        private void CalculateScores(SeoAuditResult r)
        {
            // Metadata
            r.MetadataScore = CalculateCategoryScore(new[] {
                r.MetaTitleStatus, r.MetaDescriptionStatus, r.MetaRobotsStatus,
                r.MetaViewportStatus, r.MetaKeywordsStatus, r.CharsetStatus, r.LanguageStatus
            });

            // Content (Includes Structure, Quality, Forms, Media)
            r.ContentScore = CalculateCategoryScore(new[] {
                r.H1Status, r.HeadingStructureStatus, r.WordCountStatus, r.ParagraphStatus, r.ListStatus,
                r.RatioStatus, r.ContentQualityStatus, r.FormStatus, r.VideoStatus, r.AudioStatus
            });

            // Technical (Includes Tech SEO, Deprecations, Images, Schema)
            r.TechnicalScore = CalculateCategoryScore(new[] {
                r.CanonicalStatus, r.HreflangStatus, r.FaviconStatus, r.PreconnectStatus, r.AmpStatus, r.RssStatus,
                r.DeprecationStatus, r.StructuredDataStatus,
                r.ImageAltStatus, r.ImageDimensionStatus, r.ImageFilenameStatus, r.ImageLazyLoadStatus, r.ImageFormatStatus, r.ImageResponsiveStatus
            });

            // Social
            r.SocialScore = CalculateCategoryScore(new[] {
                r.OgTitleStatus, r.OgDescriptionStatus, r.OgImageStatus, r.OgTypeStatus, r.TwitterStatus, r.FacebookStatus
            });

            // Accessibility
            r.AccessibilityScore = CalculateCategoryScore(new[] {
                r.AccessibilityStatus
            });

            // Overall Score (Weighted Average or Aggregate)
            var allStatuses = new List<string>();
            allStatuses.AddRange(new[] { r.MetaTitleStatus, r.MetaDescriptionStatus, r.MetaRobotsStatus, r.MetaViewportStatus, r.MetaKeywordsStatus, r.CharsetStatus, r.LanguageStatus });
            allStatuses.AddRange(new[] { r.H1Status, r.HeadingStructureStatus, r.WordCountStatus, r.ParagraphStatus, r.ListStatus, r.RatioStatus, r.ContentQualityStatus, r.FormStatus, r.VideoStatus, r.AudioStatus });
            allStatuses.AddRange(new[] { r.CanonicalStatus, r.HreflangStatus, r.FaviconStatus, r.PreconnectStatus, r.AmpStatus, r.RssStatus, r.DeprecationStatus, r.StructuredDataStatus, r.ImageAltStatus, r.ImageDimensionStatus, r.ImageFilenameStatus, r.ImageLazyLoadStatus, r.ImageFormatStatus, r.ImageResponsiveStatus });
            allStatuses.AddRange(new[] { r.OgTitleStatus, r.OgDescriptionStatus, r.OgImageStatus, r.OgTypeStatus, r.TwitterStatus, r.FacebookStatus });
            allStatuses.AddRange(new[] { r.AccessibilityStatus });

            r.OverallScore = CalculateCategoryScore(allStatuses.ToArray());
        }

        private int CalculateCategoryScore(string[] statuses)
        {
            double points = 0;
            double maxPotential = 0;
            double minPotential = 0;

            // Filter out Info/Unknown from the denominator
            var validItems = statuses.Where(s => !s.StartsWith("Info") && !s.StartsWith("Unknown")).ToList();

            if (!validItems.Any()) return 0;

            foreach (var s in validItems)
            {
                maxPotential += 5;   // Best case (+5)
                minPotential += -10; // Worst case (-10)

                if (s.StartsWith("Good")) points += 5;
                else if (s.StartsWith("Warning")) points += -3;
                else if (s.StartsWith("Critical")) points += -10;
                // Info adds 0 points but isn't in this loop
            }

            // Normalize to 0-100 scale
            // Formula: ( (Actual - Min) / (Max - Min) ) * 100
            double range = maxPotential - minPotential;
            if (range == 0) return 0;

            double normalized = ((points - minPotential) / range) * 100;
            return (int)Math.Max(0, Math.Min(100, Math.Round(normalized)));
        }

        private void CheckMetaHelpers(HtmlDocument doc, SeoAuditResult result)
        {
            var robots = doc.DocumentNode.SelectSingleNode("//meta[@name='robots']")?.GetAttributeValue("content", "");
            result.MetaRobots = robots;
            if (string.IsNullOrEmpty(robots)) result.MetaRobotsStatus = "Good (Default)";
            else if (robots.Contains("noindex")) result.MetaRobotsStatus = "Critical (NoIndex)";
            else result.MetaRobotsStatus = "Good";

            var vp = doc.DocumentNode.SelectSingleNode("//meta[@name='viewport']");
            result.MetaViewport = vp?.GetAttributeValue("content", "");
            result.MetaViewportStatus = vp == null ? "Critical (Missing)" : "Good";

            result.HasMetaKeywords = doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']") != null;

            var charset = doc.DocumentNode.SelectSingleNode("//meta[@charset]")?.GetAttributeValue("charset", "");
            result.Charset = charset;
            result.CharsetStatus = string.IsNullOrEmpty(charset) ? "Critical (Missing)" : "Good";

            var lang = doc.DocumentNode.SelectSingleNode("//html")?.GetAttributeValue("lang", "");
            result.Language = lang;
            result.LanguageStatus = string.IsNullOrEmpty(lang) ? "Warning (Missing)" : "Good";
        }

        private void CheckSocialAndTechnical(string url, HtmlDocument doc, SeoAuditResult result)
        {
            result.OgTitle = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", "");
            result.OgTitleStatus = string.IsNullOrEmpty(result.OgTitle) ? "Warning" : "Good";

            result.OgDescription = doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']")?.GetAttributeValue("content", "");
            result.OgDescriptionStatus = string.IsNullOrEmpty(result.OgDescription) ? "Warning" : "Good";

            result.OgImage = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", "");
            result.OgImageStatus = string.IsNullOrEmpty(result.OgImage) ? "Critical" : "Good";

            result.OgType = doc.DocumentNode.SelectSingleNode("//meta[@property='og:type']")?.GetAttributeValue("content", "");
            result.OgTypeStatus = string.IsNullOrEmpty(result.OgType) ? "Warning" : "Good";

            result.FbAppId = doc.DocumentNode.SelectSingleNode("//meta[@property='fb:app_id']")?.GetAttributeValue("content", "");
            result.ArticlePublisher = doc.DocumentNode.SelectSingleNode("//meta[@property='article:publisher']")?.GetAttributeValue("content", "");
            result.FacebookStatus = (!string.IsNullOrEmpty(result.FbAppId) || !string.IsNullOrEmpty(result.ArticlePublisher)) ? "Good" : "Info";

            result.HasTwitterCard = doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:card']") != null;
            result.TwitterStatus = result.HasTwitterCard ? "Good" : "Warning";

            var canonical = doc.DocumentNode.SelectSingleNode("//link[@rel='canonical']")?.GetAttributeValue("href", "");
            result.FoundCanonical = canonical;
            if (string.IsNullOrEmpty(canonical)) result.CanonicalStatus = "Critical";
            else if (url.Contains(canonical, StringComparison.OrdinalIgnoreCase)) { result.IsCanonicalCorrect = true; result.CanonicalStatus = "Good"; }
            else { result.IsCanonicalCorrect = false; result.CanonicalStatus = "Warning"; }

            result.HreflangCount = doc.DocumentNode.SelectNodes("//link[@rel='alternate' and @hreflang]")?.Count ?? 0;
            result.HreflangStatus = result.HreflangCount > 0 ? "Good" : "Info";

            result.FaviconUrl = doc.DocumentNode.SelectSingleNode("//link[contains(@rel,'icon')]")?.GetAttributeValue("href", "");
            result.FaviconStatus = string.IsNullOrEmpty(result.FaviconUrl) ? "Warning" : "Good";

            result.PreconnectCount = doc.DocumentNode.SelectNodes("//link[@rel='preconnect']")?.Count ?? 0;
            result.PreconnectStatus = result.PreconnectCount > 0 ? "Good" : "Info";

            var amp = doc.DocumentNode.SelectSingleNode("//link[@rel='amphtml']");
            result.AmpUrl = amp?.GetAttributeValue("href", "");
            result.AmpStatus = amp != null ? "Info" : "Info";

            var rss = doc.DocumentNode.SelectSingleNode("//link[@rel='alternate' and contains(@type, 'rss')]");
            result.RssFeedUrl = rss?.GetAttributeValue("href", "");
            result.RssStatus = rss != null ? "Good" : "Warning";
        }

        private void CheckMedia(HtmlDocument doc, SeoAuditResult result)
        {
            var vids = doc.DocumentNode.SelectNodes("//video|//iframe[contains(@src,'youtube') or contains(@src,'vimeo')]");
            result.VideoCount = vids?.Count ?? 0;
            result.VideoStatus = result.VideoCount > 0 ? "Good" : "Info";

            var audio = doc.DocumentNode.SelectNodes("//audio");
            result.AudioCount = audio?.Count ?? 0;
            result.AudioStatus = result.AudioCount > 0 ? "Good" : "Info";
        }

        private void AnalyzeTextQuality(HtmlDocument doc, SeoAuditResult result, int htmlLength)
        {
            result.TotalHtmlSize = htmlLength;
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode == null) return;

            var pNodes = bodyNode.SelectNodes("//p");
            result.ParagraphCount = pNodes?.Count ?? 0;
            string pText = pNodes != null ? string.Join(" ", pNodes.Select(p => p.InnerText)) : "";

            var cleanup = bodyNode.SelectNodes("//script|//style|//noscript|//iframe|//svg");
            if (cleanup != null) foreach (var n in cleanup) n.Remove();

            string rawText = System.Net.WebUtility.HtmlDecode(bodyNode.InnerText);
            rawText = Regex.Replace(rawText, @"\s+", " ").Trim();

            result.TextContentSize = rawText.Length;
            if (result.TotalHtmlSize > 0)
                result.TextToHtmlRatio = Math.Round((double)result.TextContentSize / result.TotalHtmlSize * 100, 2);
            result.RatioStatus = result.TextToHtmlRatio < 10 ? "Warning" : "Good";

            var words = rawText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            result.WordCount = words.Length;
            if (result.WordCount < 300) result.WordCountStatus = "Warning";
            else result.WordCountStatus = "Good";

            if (rawText.Length > 0)
                result.TextInParagraphsPercentage = Math.Round((double)pText.Length / rawText.Length * 100, 2);
            result.ParagraphStatus = result.TextInParagraphsPercentage < 50 ? "Warning" : "Good";

            if (words.Length > 0)
            {
                var sentences = Regex.Split(rawText, @"(?<=[.!?])\s+");
                int sentenceCount = sentences.Length > 0 ? sentences.Length : 1;

                int syllableCount = 0;
                foreach (string w in words) syllableCount += CountSyllables(w);

                double avgSentenceLen = (double)words.Length / sentenceCount;
                double avgSylPerWord = (double)syllableCount / words.Length;

                result.AverageSentenceLength = Math.Round(avgSentenceLen, 1);
                result.FleschReadingEase = Math.Round(206.835 - (1.015 * avgSentenceLen) - (84.6 * avgSylPerWord), 1);

                if (result.FleschReadingEase < 30) result.ContentQualityStatus = "Warning (Very Difficult)";
                else if (result.FleschReadingEase > 60) result.ContentQualityStatus = "Good";
                else result.ContentQualityStatus = "Acceptable";
            }
        }

        private int CountSyllables(string word)
        {
            word = word.ToLower().Trim();
            if (word.Length <= 3) return 1;
            word = Regex.Replace(word, "(?:[^laeiouy]es|ed|[^laeiouy]e)$", "");
            word = Regex.Replace(word, "^y", "");
            var matches = Regex.Matches(word, "[aeiouy]{1,2}");
            return Math.Max(1, matches.Count);
        }
    }
}