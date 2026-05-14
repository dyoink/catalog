using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AnyAdmin")]
public class AboutController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly IFileUploadService _upload;

    public AboutController(ISettingsService settingsService, IFileUploadService upload)
    {
        _settingsService = settingsService;
        _upload = upload;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> Update(AquaCMS.Models.Entities.SiteSettings settings, IFormFile? aboutImageFile)
    {
        var existing = await _settingsService.GetSettingsAsync();
        
        if (aboutImageFile is { Length: > 0 })
            existing.AboutImage = await _upload.UploadImageAsync(aboutImageFile, "settings") ?? existing.AboutImage;

        existing.AboutTitle = settings.AboutTitle;
        existing.AboutContent = settings.AboutContent;

        await _settingsService.UpdateSettingsAsync(existing);
        TempData["Success"] = "Cập nhật nội dung giới thiệu thành công!";
        return RedirectToAction(nameof(Index));
    }
}
