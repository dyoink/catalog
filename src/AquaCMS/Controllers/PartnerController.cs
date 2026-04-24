using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Controllers;

/// <summary>
/// Controller đối tác — listing, detail.
/// URL: /doi-tac, /doi-tac/{slug}
/// </summary>
public class PartnerController : Controller
{
    private readonly IPartnerService _partnerService;
    private readonly ISettingsService _settingsService;

    public PartnerController(
        IPartnerService partnerService,
        ISettingsService settingsService)
    {
        _partnerService = partnerService;
        _settingsService = settingsService;
    }

    /// <summary>GET /doi-tac — Danh sách đối tác</summary>
    public async Task<IActionResult> Index(string? category, int page = 1)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var categories = await _partnerService.GetCategoriesAsync();
        var partners = await _partnerService.GetActivePartnersAsync(
            categorySlug: category, page: page, pageSize: 12);

        ViewData["Title"] = $"Đối tác | {settings.CompanyName}";
        ViewData["MetaDescription"] = "Mạng lưới đối tác chiến lược";
        ViewData["Categories"] = categories;

        return View(partners);
    }

    /// <summary>GET /doi-tac/{slugAndId} — Chi tiết đối tác.</summary>
    public async Task<IActionResult> Detail(string slugAndId)
    {
        if (string.IsNullOrWhiteSpace(slugAndId)) return NotFound();
        var (urlSlug, shortId) = ParseSlugAndId(slugAndId);

        Models.Entities.Partner? partner;
        if (shortId == null)
        {
            partner = await _partnerService.GetBySlugAsync(slugAndId);
            if (partner == null) return NotFound();
            return RedirectPermanent($"/doi-tac/{partner.Slug}-{partner.ShortId}");
        }

        partner = await _partnerService.GetByShortIdAsync(shortId.Value);
        if (partner == null) return NotFound();

        if (!string.Equals(partner.Slug, urlSlug, StringComparison.OrdinalIgnoreCase))
            return RedirectPermanent($"/doi-tac/{partner.Slug}-{partner.ShortId}");

        var settings = await _settingsService.GetSettingsAsync();

        ViewData["Title"] = $"{partner.Name} | {settings.CompanyName}";
        ViewData["MetaDescription"] = partner.Description;
        ViewData["OgImage"] = partner.Image;
        ViewData["CanonicalUrl"] = $"/doi-tac/{partner.Slug}-{partner.ShortId}";

        return View(partner);
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
