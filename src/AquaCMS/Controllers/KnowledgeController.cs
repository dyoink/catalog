using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Controllers;

/// <summary>
/// Controller kiến thức — danh sách bài viết, chi tiết bài viết.
/// URL: /kien-thuc, /kien-thuc/{slug}
/// </summary>
public class KnowledgeController : Controller
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly ISettingsService _settingsService;

    public KnowledgeController(
        IKnowledgeService knowledgeService,
        ISettingsService settingsService)
    {
        _knowledgeService = knowledgeService;
        _settingsService = settingsService;
    }

    /// <summary>GET /kien-thuc — Danh sách bài viết</summary>
    public async Task<IActionResult> Index(string? category, int page = 1)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var categories = await _knowledgeService.GetCategoriesAsync();
        var posts = await _knowledgeService.GetPublishedPostsAsync(
            categorySlug: category, page: page, pageSize: 12);

        ViewData["Title"] = $"Kiến thức | {settings.CompanyName}";
        ViewData["MetaDescription"] = "Bài viết, hướng dẫn kỹ thuật nuôi trồng thủy sản";
        ViewData["Categories"] = categories;
        ViewData["CurrentCategory"] = category;

        return View(posts);
    }

    /// <summary>GET /kien-thuc/{slugAndId} — Chi tiết bài viết. URL: /kien-thuc/cach-cho-ca-an-12345</summary>
    public async Task<IActionResult> Detail(string slugAndId)
    {
        if (string.IsNullOrWhiteSpace(slugAndId)) return NotFound();

        var (urlSlug, shortId) = ParseSlugAndId(slugAndId);

        Models.Entities.Post? post;
        if (shortId == null)
        {
            // Backward compat — URL chỉ có slug
            post = await _knowledgeService.GetBySlugAsync(slugAndId);
            if (post == null) return NotFound();
            return RedirectPermanent($"/kien-thuc/{post.Slug}-{post.ShortId}");
        }

        post = await _knowledgeService.GetByShortIdAsync(shortId.Value);
        if (post == null) return NotFound();

        // Slug khác → 301 redirect canonical
        if (!string.Equals(post.Slug, urlSlug, StringComparison.OrdinalIgnoreCase))
            return RedirectPermanent($"/kien-thuc/{post.Slug}-{post.ShortId}");

        var settings = await _settingsService.GetSettingsAsync();
        _ = _knowledgeService.IncrementViewCountAsync(post.Id);

        ViewData["Title"] = $"{post.MetaTitle ?? post.Title} | {settings.CompanyName}";
        ViewData["MetaDescription"] = post.MetaDesc ?? post.Excerpt;
        ViewData["OgType"] = "article";
        ViewData["OgImage"] = post.Image;
        ViewData["CanonicalUrl"] = $"/kien-thuc/{post.Slug}-{post.ShortId}";

        return View(post);
    }

    private static (string slug, long? shortId) ParseSlugAndId(string slugAndId)
    {
        var lastDash = slugAndId.LastIndexOf('-');
        if (lastDash <= 0 || lastDash == slugAndId.Length - 1) return (slugAndId, null);
        var idPart = slugAndId[(lastDash + 1)..];
        if (long.TryParse(idPart, out var id)) return (slugAndId[..lastDash], id);
        return (slugAndId, null);
    }
}
