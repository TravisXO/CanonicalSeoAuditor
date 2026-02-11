using Microsoft.AspNetCore.Mvc;
using CanonicalSeoAuditor.Services; // For CrawlerClientService
using SeoAuditor.Crawler.Services;  // POINTS TO NEW LIBRARY
using SeoAuditor.Crawler.Models;    // POINTS TO NEW LIBRARY

namespace CanonicalSeoAuditor.Controllers
{
    public class AuditController : Controller
    {
        private readonly SeoAuditService _seoService;
        private readonly CrawlerClientService _crawlerService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(SeoAuditService seoService, CrawlerClientService crawlerService, ILogger<AuditController> logger)
        {
            _seoService = seoService;
            _crawlerService = crawlerService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Run(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                ViewBag.ErrorMessage = "Please enter a valid URL.";
                return View("Index");
            }

            // Ensure protocol
            if (!url.StartsWith("http")) url = "https://" + url;

            _logger.LogInformation("Starting audit for user-provided URL: {Url}", url);

            try
            {
                // 1. Get HTML from Node.js Crawler
                var crawlerResult = await _crawlerService.GetRenderedHtmlAsync(url);

                if (crawlerResult == null || string.IsNullOrEmpty(crawlerResult.Content))
                {
                    ViewBag.ErrorMessage = "Unable to crawl the page. It might be blocking automated tools or timed out.";
                    return View("Index");
                }

                // 2. Analyze HTML using the C# Service (Now in the Library)
                var auditResult = _seoService.AuditHtml(url, crawlerResult.Content);

                // 3. Show Results
                return View("Results", auditResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during audit of {Url}", url);
                ViewBag.ErrorMessage = "An unexpected error occurred while analyzing the site. Please try again.";
                return View("Index");
            }
        }
    }
}