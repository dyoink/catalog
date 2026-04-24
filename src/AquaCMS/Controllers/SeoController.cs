using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Controllers;

/// <summary>
/// Trả về sitemap.xml và robots.txt cho SEO crawlers (Google, Bing).
/// </summary>
public class SeoController : Controller
{
    private readonly ISitemapService _sitemap;
    private readonly ISettingsService _settings;
    private readonly ILogger<SeoController> _logger;

    public SeoController(ISitemapService sitemap, ISettingsService settings, ILogger<SeoController> logger)
    {
        _sitemap = sitemap;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>GET /sitemap.xml</summary>
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Sitemap()
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var xml = await _sitemap.GenerateSitemapAsync(baseUrl);
            return Content(xml, "application/xml; charset=utf-8");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi generate sitemap");
            return Content("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"></urlset>", "application/xml");
        }
    }

    /// <summary>GET /robots.txt</summary>
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var content = $@"User-agent: *
Allow: /
Disallow: /admin/
Disallow: /dang-nhap
Disallow: /api/
Disallow: /khong-co-quyen

Sitemap: {baseUrl}/sitemap.xml
";
        return Content(content, "text/plain; charset=utf-8");
    }
}
