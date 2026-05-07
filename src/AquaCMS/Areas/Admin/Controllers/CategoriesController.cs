using AquaCMS.Helpers;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>Admin CRUD danh mục sản phẩm.</summary>
[Area("Admin")]
[Authorize(Policy = "ManagerUp")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IActivityLogService _activity;

    public CategoriesController(
        ICategoryService categoryService, 
        IFileUploadService fileUploadService,
        IActivityLogService activity)
    {
        _categoryService = categoryService;
        _fileUploadService = fileUploadService;
        _activity = activity;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Quản lý danh mục";
        return View(await _categoryService.GetAllWithCountAsync());
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm danh mục";
        return View(new Category());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category, IFormFile? imageFile)
    {
        if (imageFile != null)
        {
            try
            {
                var imageUrl = await _fileUploadService.UploadImageAsync(imageFile, "categories");
                if (imageUrl != null) category.Image = imageUrl;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Image", ex.Message);
                return View(category);
            }
        }

        if (string.IsNullOrEmpty(category.Slug))
            category.Slug = SlugHelper.GenerateSlug(category.Name);

        category.CreatedAt = DateTime.UtcNow;
        await _categoryService.CreateAsync(category);
        await _activity.LogAsync("CREATE", "Category", category.Id.ToString(), category.Name);
        
        TempData["Success"] = "Đã thêm danh mục!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null) return NotFound();

        ViewData["Title"] = $"Sửa: {category.Name}";
        return View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Category category, IFormFile? imageFile)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        if (imageFile != null)
        {
            try
            {
                var imageUrl = await _fileUploadService.UploadImageAsync(imageFile, "categories");
                if (imageUrl != null)
                {
                    // Xóa ảnh cũ nếu là ảnh local
                    if (!string.IsNullOrEmpty(existing.Image))
                        _fileUploadService.DeleteFile(existing.Image);

                    existing.Image = imageUrl;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Image", ex.Message);
                return View(category);
            }
        }
        else if (!string.IsNullOrEmpty(category.Image))
        {
            // Nếu không upload file mới nhưng có nhập URL mới
            existing.Image = category.Image;
        }

        existing.Name = category.Name;
        existing.Slug = string.IsNullOrEmpty(category.Slug) ? SlugHelper.GenerateSlug(category.Name) : category.Slug;
        existing.SortOrder = category.SortOrder;

        await _categoryService.UpdateAsync(existing);
        await _activity.LogAsync("UPDATE", "Category", existing.Id.ToString(), existing.Name);
        
        TempData["Success"] = "Đã cập nhật danh mục!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing != null)
        {
            await _categoryService.DeleteAsync(id);
            await _activity.LogAsync("DELETE", "Category", id.ToString(), existing.Name);
            TempData["Success"] = "Đã xóa danh mục!";
        }
        return RedirectToAction(nameof(Index));
    }
}
