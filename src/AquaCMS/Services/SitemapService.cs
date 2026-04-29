using System.Text;
using System.Xml;
using AquaCMS.Data;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AquaCMS.Services;

/// <summary>
/// Implementation sitemap.xml generator — cache 1 giờ.
/// </summary>
public class SitemapService : ISitemapService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SitemapService> _logger;
    private const string CacheKey = "sitemap_xml";

    public SitemapService(AppDbContext db, IMemoryCache cache, ILogger<SitemapService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GenerateSitemapAsync(string baseUrl)
    {
        var cacheKey = $"{CacheKey}_{baseUrl}";
        if (_cache.TryGetValue(cacheKey, out string? cached) && cached != null)
            return cached;

        baseUrl = baseUrl.TrimEnd('/');

        var sb = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            Async = true
        };

        using (var sw = new StringWriter(sb))
        using (var w = XmlWriter.Create(sw, settings))
        {
            await w.WriteStartDocumentAsync();
            w.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            // Static pages
            WriteUrl(w, baseUrl + "/", DateTime.UtcNow, "daily", "1.0");
            WriteUrl(w, baseUrl + "/san-pham", DateTime.UtcNow, "daily", "0.9");
            WriteUrl(w, baseUrl + "/kien-thuc", DateTime.UtcNow, "daily", "0.8");
            WriteUrl(w, baseUrl + "/doi-tac", DateTime.UtcNow, "weekly", "0.7");

            try
            {
                // Categories
                var cats = await _db.Categories.AsNoTracking()
                    .Select(c => new { c.Slug, c.CreatedAt }).ToListAsync();
                foreach (var c in cats)
                    WriteUrl(w, $"{baseUrl}/danh-muc/{c.Slug}", c.CreatedAt, "weekly", "0.7");

                // Products
                var products = await _db.Products.AsNoTracking()
                    .Where(p => p.Status != ProductStatus.Hidden)
                    .Select(p => new { Slug = p.Metadata != null ? p.Metadata.Slug : "", p.ShortId, p.UpdatedAt }).ToListAsync();
                foreach (var p in products)
                    if (!string.IsNullOrEmpty(p.Slug))
                        WriteUrl(w, $"{baseUrl}/san-pham/{p.Slug}-{p.ShortId}", p.UpdatedAt, "weekly", "0.8");

                // Posts
                var posts = await _db.Posts.AsNoTracking()
                    .Where(p => p.IsPublished)
                    .Select(p => new { p.Slug, p.ShortId, p.UpdatedAt }).ToListAsync();
                foreach (var p in posts)
                    WriteUrl(w, $"{baseUrl}/kien-thuc/{p.Slug}-{p.ShortId}", p.UpdatedAt, "monthly", "0.7");

                // Partners
                var partners = await _db.Partners.AsNoTracking()
                    .Where(p => p.IsActive)
                    .Select(p => new { p.Slug, p.ShortId, p.UpdatedAt }).ToListAsync();
                foreach (var p in partners)
                    WriteUrl(w, $"{baseUrl}/doi-tac/{p.Slug}-{p.ShortId}", p.UpdatedAt, "monthly", "0.6");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi generate sitemap entries");
            }

            await w.WriteEndElementAsync(); // urlset
            await w.WriteEndDocumentAsync();
        }

        var xml = sb.ToString();
        _cache.Set(cacheKey, xml, TimeSpan.FromHours(1));
        return xml;
    }

    private static void WriteUrl(XmlWriter w, string loc, DateTime lastMod, string changeFreq, string priority)
    {
        w.WriteStartElement("url");
        w.WriteElementString("loc", loc);
        w.WriteElementString("lastmod", lastMod.ToString("yyyy-MM-dd"));
        w.WriteElementString("changefreq", changeFreq);
        w.WriteElementString("priority", priority);
        w.WriteEndElement();
    }
}
