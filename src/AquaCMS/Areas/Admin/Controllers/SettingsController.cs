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
            if (logoFile is { Length: > 0 })
                settings.Logo = await _upload.UploadImageAsync(logoFile, "settings") ?? settings.Logo;

            if (heroImageFile is { Length: > 0 })
                settings.HeroBackgroundImage = await _upload.UploadImageAsync(heroImageFile, "settings") ?? settings.HeroBackgroundImage;

            if (aboutImageFile is { Length: > 0 })
                settings.AboutImage = await _upload.UploadImageAsync(aboutImageFile, "settings") ?? settings.AboutImage;

            if (ogImageFile is { Length: > 0 })
                settings.DefaultOgImage = await _upload.UploadImageAsync(ogImageFile, "settings") ?? settings.DefaultOgImage;

            settings.HeroDescription = _sanitizer.Sanitize(settings.HeroDescription);
            settings.AboutContent = _sanitizer.Sanitize(settings.AboutContent);
            settings.FooterAboutText = _sanitizer.Sanitize(settings.FooterAboutText);

            await _settingsService.UpdateSettingsAsync(settings);
            await _activity.LogAsync("UPDATE", "SiteSettings", settings.Id.ToString(), "Cập nhật cài đặt site");
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
