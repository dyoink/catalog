using AquaCMS.Helpers;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>Admin CRUD đối tác.</summary>
[Area("Admin")]
[Authorize(Policy = "EditorUp")]
public class PartnersController : Controller
{
    private readonly IPartnerService _partnerService;
    private readonly IFileUploadService _upload;
    private readonly IHtmlSanitizerService _sanitizer;
    private readonly IActivityLogService _activity;
    private readonly ILogger<PartnersController> _logger;

    public PartnersController(
        IPartnerService partnerService,
        IFileUploadService upload,
        IHtmlSanitizerService sanitizer,
        IActivityLogService activity,
        ILogger<PartnersController> logger)
    {
        _partnerService = partnerService;
        _upload = upload;
        _sanitizer = sanitizer;
        _activity = activity;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Quản lý đối tác";
        ViewBag.Search = search;
        ViewData["Categories"] = await _partnerService.GetCategoriesAsync();
        return View(await _partnerService.GetAdminPartnersAsync(search, page));
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Thêm đối tác";
        ViewData["Categories"] = await _partnerService.GetCategoriesAsync();
        return View(new Partner());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Create(Partner partner, IFormFile? imageFile)
    {
        try
        {
            if (string.IsNullOrEmpty(partner.Slug))
                partner.Slug = SlugHelper.GenerateSlug(partner.Name);

            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "partners");
                if (!string.IsNullOrEmpty(url)) partner.Image = url;
            }

            partner.DetailedDescription = _sanitizer.Sanitize(partner.DetailedDescription);
            partner.CreatedAt = DateTime.UtcNow;
            partner.UpdatedAt = DateTime.UtcNow;

            await _partnerService.CreateAsync(partner);
            await _activity.LogAsync("CREATE", "Partner", partner.Id.ToString(), partner.Name);
            TempData["Success"] = "Đã thêm đối tác thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tạo partner");
            TempData["Error"] = "Không thể tạo đối tác.";
        }
        ViewData["Categories"] = await _partnerService.GetCategoriesAsync();
        return View(partner);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var partner = await _partnerService.GetByIdAsync(id);
        if (partner == null) return NotFound();

        ViewData["Title"] = $"Sửa: {partner.Name}";
        ViewData["Categories"] = await _partnerService.GetCategoriesAsync();
        return View(partner);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Edit(Guid id, Partner partner, IFormFile? imageFile)
    {
        var existing = await _partnerService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        try
        {
            existing.Name = partner.Name;
            existing.Slug = string.IsNullOrEmpty(partner.Slug) ? SlugHelper.GenerateSlug(partner.Name) : partner.Slug;
            existing.Description = partner.Description;
            existing.DetailedDescription = _sanitizer.Sanitize(partner.DetailedDescription);
            existing.PartnerCategoryId = partner.PartnerCategoryId;
            existing.Location = partner.Location;
            existing.Since = partner.Since;
            existing.ContactEmail = partner.ContactEmail;
            existing.ContactPhone = partner.ContactPhone;
            existing.Website = partner.Website;
            existing.IsActive = partner.IsActive;
            existing.SortOrder = partner.SortOrder;
            existing.UpdatedAt = DateTime.UtcNow;

            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "partners");
                if (!string.IsNullOrEmpty(url))
                {
                    _upload.DeleteFile(existing.Image);
                    existing.Image = url;
                }
            }
            else if (!string.IsNullOrEmpty(partner.Image))
            {
                existing.Image = partner.Image;
            }

            await _partnerService.UpdateAsync(existing);
            await _activity.LogAsync("UPDATE", "Partner", existing.Id.ToString(), existing.Name);
            TempData["Success"] = "Đã cập nhật đối tác!";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật partner {Id}", id);
            TempData["Error"] = "Không thể cập nhật đối tác.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _partnerService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        try
        {
            _upload.DeleteFile(existing.Image);
            await _partnerService.DeleteAsync(id);
            await _activity.LogAsync("DELETE", "Partner", id.ToString(), existing.Name);
            TempData["Success"] = "Đã xóa đối tác!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xóa partner {Id}", id);
            TempData["Error"] = "Không thể xóa đối tác.";
        }
        return RedirectToAction(nameof(Index));
    }
}
