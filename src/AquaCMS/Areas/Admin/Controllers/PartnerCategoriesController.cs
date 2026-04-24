using AquaCMS.Helpers;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>Admin CRUD danh mục đối tác.</summary>
[Area("Admin")]
[Authorize(Policy = "ManagerUp")]
public class PartnerCategoriesController : Controller
{
    private readonly IPartnerService _service;
    private readonly IActivityLogService _activity;
    private readonly ILogger<PartnerCategoriesController> _logger;

    public PartnerCategoriesController(
        IPartnerService service, IActivityLogService activity, ILogger<PartnerCategoriesController> logger)
    {
        _service = service; _activity = activity; _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Danh mục đối tác";
        return View(await _service.GetCategoriesAsync());
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm danh mục đối tác";
        return View(new PartnerCategory());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartnerCategory cat)
    {
        try
        {
            if (string.IsNullOrEmpty(cat.Slug))
                cat.Slug = SlugHelper.GenerateSlug(cat.Name);
            cat.CreatedAt = DateTime.UtcNow;
            await _service.CreateCategoryAsync(cat);
            await _activity.LogAsync("CREATE", "PartnerCategory", cat.Id.ToString(), cat.Name);
            TempData["Success"] = "Đã thêm danh mục!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tạo partner category");
            TempData["Error"] = "Không thể tạo danh mục.";
        }
        return View(cat);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var cat = await _service.GetCategoryByIdAsync(id);
        if (cat == null) return NotFound();
        ViewData["Title"] = $"Sửa: {cat.Name}";
        return View(cat);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PartnerCategory cat)
    {
        var existing = await _service.GetCategoryByIdAsync(id);
        if (existing == null) return NotFound();
        try
        {
            existing.Name = cat.Name;
            existing.Slug = string.IsNullOrEmpty(cat.Slug) ? SlugHelper.GenerateSlug(cat.Name) : cat.Slug;
            existing.SortOrder = cat.SortOrder;
            await _service.UpdateCategoryAsync(existing);
            await _activity.LogAsync("UPDATE", "PartnerCategory", id.ToString(), existing.Name);
            TempData["Success"] = "Đã cập nhật!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật partner category {Id}", id);
            TempData["Error"] = "Không thể cập nhật.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.DeleteCategoryAsync(id);
            await _activity.LogAsync("DELETE", "PartnerCategory", id.ToString());
            TempData["Success"] = "Đã xóa danh mục!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xóa partner category {Id}", id);
            TempData["Error"] = "Không thể xóa (có thể vẫn còn đối tác liên kết).";
        }
        return RedirectToAction(nameof(Index));
    }
}
