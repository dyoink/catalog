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

    public CategoriesController(ICategoryService categoryService)
        => _categoryService = categoryService;

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
    public async Task<IActionResult> Create(Category category)
    {
        if (string.IsNullOrEmpty(category.Slug))
            category.Slug = SlugHelper.GenerateSlug(category.Name);

        category.CreatedAt = DateTime.UtcNow;
        await _categoryService.CreateAsync(category);
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
    public async Task<IActionResult> Edit(Guid id, Category category)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Name = category.Name;
        existing.Slug = string.IsNullOrEmpty(category.Slug) ? SlugHelper.GenerateSlug(category.Name) : category.Slug;
        existing.Image = category.Image;
        existing.SortOrder = category.SortOrder;

        await _categoryService.UpdateAsync(existing);
        TempData["Success"] = "Đã cập nhật danh mục!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteAsync(id);
        TempData["Success"] = "Đã xóa danh mục!";
        return RedirectToAction(nameof(Index));
    }
}
