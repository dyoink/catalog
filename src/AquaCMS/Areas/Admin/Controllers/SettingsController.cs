using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>
/// Admin cài đặt site — chỉ SUPER_ADMIN.
/// Quản lý: thông tin công ty, social, hero/about, SEO, footer, module toggles, upload ảnh.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "SuperAdmin")]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly IFileUploadService _upload;
    private readonly IHtmlSanitizerService _sanitizer;
    private readonly IActivityLogService _activity;
    private readonly IEmailService _email;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISettingsService settingsService,
        IFileUploadService upload,
        IHtmlSanitizerService sanitizer,
        IActivityLogService activity,
        IEmailService email,
        ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _upload = upload;
        _sanitizer = sanitizer;
        _activity = activity;
        _email = email;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Cài đặt hệ thống";
        var settings = await _settingsService.GetSettingsAsync();
        return View(settings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Index(
        Models.Entities.SiteSettings settings,
        IFormFile? logoFile,
        IFormFile? heroImageFile,
        IFormFile? aboutImageFile,
        IFormFile? ogImageFile)
    {
        try
        {
            var existing = await _settingsService.GetSettingsAsync();

            // Map manually to avoid overwriting or missing fields
            existing.CompanyName = settings.CompanyName;
            existing.Phone = settings.Phone;
            existing.Email = settings.Email;
            existing.Address = settings.Address;
            existing.Logo = settings.Logo;

            existing.Facebook = settings.Facebook;
            existing.ShowFacebook = settings.ShowFacebook;
            existing.Zalo = settings.Zalo;
            existing.ShowZalo = settings.ShowZalo;
            existing.Youtube = settings.Youtube;
            existing.ShowYoutube = settings.ShowYoutube;
            existing.Tiktok = settings.Tiktok;
            existing.ShowTiktok = settings.ShowTiktok;
            existing.Telegram = settings.Telegram;
            existing.ShowTelegram = settings.ShowTelegram;
            existing.ShowHotline = settings.ShowHotline;

            existing.BackgroundColor = settings.BackgroundColor;
            existing.PrimaryColor = settings.PrimaryColor;
            existing.FooterText = settings.FooterText;
            existing.ShowFooter = settings.ShowFooter;

            existing.ShowBanners = settings.ShowBanners;
            existing.ShowCategories = settings.ShowCategories;
            existing.ShowFeaturedProducts = settings.ShowFeaturedProducts;
            existing.ShowLatestPosts = settings.ShowLatestPosts;
            existing.ShowPartners = settings.ShowPartners;
            existing.FeaturedProductsCount = settings.FeaturedProductsCount;
            existing.LatestPostsCount = settings.LatestPostsCount;

            existing.ShowNavProducts = settings.ShowNavProducts;
            existing.ShowNavKnowledge = settings.ShowNavKnowledge;
            existing.ShowNavPartners = settings.ShowNavPartners;
            existing.ShowNavCart = settings.ShowNavCart;

            existing.HeroTitle = settings.HeroTitle;
            existing.HeroSubtitle = settings.HeroSubtitle;
            existing.HeroDescription = _sanitizer.Sanitize(settings.HeroDescription);
            existing.HeroButtonText = settings.HeroButtonText;
            existing.HeroButtonUrl = settings.HeroButtonUrl;

            existing.AboutTitle = settings.AboutTitle;
            existing.AboutContent = _sanitizer.Sanitize(settings.AboutContent);

            existing.DefaultMetaTitle = settings.DefaultMetaTitle;
            existing.DefaultMetaDescription = settings.DefaultMetaDescription;
            existing.DefaultOgImage = settings.DefaultOgImage;
            existing.GoogleAnalyticsId = settings.GoogleAnalyticsId;
            existing.FacebookPixelId = settings.FacebookPixelId;

            existing.FooterAboutText = _sanitizer.Sanitize(settings.FooterAboutText);
            existing.CopyrightText = settings.CopyrightText;

            // Email / SMTP
            existing.EmailEnabled = settings.EmailEnabled;
            existing.SmtpHost = settings.SmtpHost;
            existing.SmtpPort = settings.SmtpPort;
            existing.SmtpUseSsl = settings.SmtpUseSsl;
            existing.SmtpUser = settings.SmtpUser;
            if (!string.IsNullOrEmpty(settings.SmtpPassword))
                existing.SmtpPassword = settings.SmtpPassword;
            existing.SmtpFromEmail = settings.SmtpFromEmail;
            existing.SmtpFromName = settings.SmtpFromName;
            existing.NotificationEmail = settings.NotificationEmail;

            existing.ChatAutoReplyMessage = settings.ChatAutoReplyMessage;

            // Files
            if (logoFile is { Length: > 0 })
                existing.Logo = await _upload.UploadImageAsync(logoFile, "settings") ?? existing.Logo;

            if (heroImageFile is { Length: > 0 })
                existing.HeroBackgroundImage = await _upload.UploadImageAsync(heroImageFile, "settings") ?? existing.HeroBackgroundImage;

            if (aboutImageFile is { Length: > 0 })
                existing.AboutImage = await _upload.UploadImageAsync(aboutImageFile, "settings") ?? existing.AboutImage;

            if (ogImageFile is { Length: > 0 })
                existing.DefaultOgImage = await _upload.UploadImageAsync(ogImageFile, "settings") ?? existing.DefaultOgImage;

            await _settingsService.UpdateSettingsAsync(existing);
            await _activity.LogAsync("UPDATE", "SiteSettings", existing.Id.ToString(), "Cập nhật cài đặt site");
            TempData["Success"] = "Đã cập nhật cài đặt!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật site settings");
            TempData["Error"] = "Đã có lỗi xảy ra khi lưu cài đặt. Vui lòng thử lại.";
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>POST /admin/settings/test-email — Gửi email test với cấu hình hiện tại.</summary>
    [HttpPost("admin/settings/test-email")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestEmail([FromForm] string to)
    {
        if (string.IsNullOrWhiteSpace(to))
            return Json(new { success = false, error = "Email người nhận trống." });

        var (ok, err) = await _email.TestConnectionAsync(to);
        return Json(new { success = ok, error = err });
    }
}
