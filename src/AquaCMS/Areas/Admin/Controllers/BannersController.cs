using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>Admin CRUD banner.</summary>
[Area("Admin")]
[Authorize(Policy = "EditorUp")]
public class BannersController : Controller
{
    private readonly IBannerService _bannerService;
    private readonly IFileUploadService _upload;
    private readonly IActivityLogService _activity;
    private readonly ILogger<BannersController> _logger;

    public BannersController(
        IBannerService bannerService,
        IFileUploadService upload,
        IActivityLogService activity,
        ILogger<BannersController> logger)
    {
        _bannerService = bannerService;
        _upload = upload;
        _activity = activity;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        ViewData["Title"] = "Quản lý Banner";
        return View(await _bannerService.GetAdminBannersAsync(page));
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm Banner";
        return View(new Banner());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Create(Banner banner, IFormFile? imageFile)
    {
        try
        {
            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "banners");
                if (!string.IsNullOrEmpty(url)) banner.Image = url;
            }
            await _bannerService.CreateAsync(banner);
            await _activity.LogAsync("CREATE", "Banner", banner.Id.ToString(), banner.Title);
            TempData["Success"] = "Đã thêm banner!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tạo banner");
            TempData["Error"] = "Không thể tạo banner.";
        }
        return View(banner);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var banner = await _bannerService.GetByIdAsync(id);
        if (banner == null) return NotFound();

        ViewData["Title"] = $"Sửa banner: {banner.Title}";
        return View(banner);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Edit(Guid id, Banner banner, IFormFile? imageFile)
    {
        var existing = await _bannerService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        try
        {
            existing.Title = banner.Title;
            existing.Subtitle = banner.Subtitle;
            existing.Description = banner.Description;
            existing.Color = banner.Color;
            existing.LinkUrl = banner.LinkUrl;
            existing.SortOrder = banner.SortOrder;
            existing.IsActive = banner.IsActive;

            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "banners");
                if (!string.IsNullOrEmpty(url))
                {
                    _upload.DeleteFile(existing.Image);
                    existing.Image = url;
                }
            }
            else if (!string.IsNullOrEmpty(banner.Image))
            {
                existing.Image = banner.Image;
            }

            await _bannerService.UpdateAsync(existing);
            await _activity.LogAsync("UPDATE", "Banner", existing.Id.ToString(), existing.Title);
            TempData["Success"] = "Đã cập nhật banner!";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật banner {Id}", id);
            TempData["Error"] = "Không thể cập nhật banner.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _bannerService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        try
        {
            _upload.DeleteFile(existing.Image);
            await _bannerService.DeleteAsync(id);
            await _activity.LogAsync("DELETE", "Banner", id.ToString(), existing.Title);
            TempData["Success"] = "Đã xóa banner!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xóa banner {Id}", id);
            TempData["Error"] = "Không thể xóa banner.";
        }
        return RedirectToAction(nameof(Index));
    }
}
