using AquaCMS.Helpers;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>
/// Admin CRUD sản phẩm — listing, create, edit, delete, bulk action.
/// Yêu cầu role Editor trở lên.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "EditorUp")]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IFileUploadService _upload;
    private readonly IActivityLogService _activity;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ICategoryService categoryService,
        IFileUploadService upload,
        IActivityLogService activity,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _categoryService = categoryService;
        _upload = upload;
        _activity = activity;
        _logger = logger;
    }

    /// <summary>GET /admin/products — Danh sách sản phẩm (admin)</summary>
    public async Task<IActionResult> Index(string? search, string? status, int page = 1)
    {
        ViewData["Title"] = "Quản lý sản phẩm";
        ViewData["Categories"] = await _categoryService.GetAllWithCountAsync();
        ViewBag.Search = search;
        ViewBag.Status = status;

        var products = await _productService.GetAdminProductsAsync(
            search: search, status: status, page: page, pageSize: 20);

        return View(products);
    }

    /// <summary>GET /admin/products/create — Form thêm sản phẩm</summary>
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Thêm sản phẩm mới";
        ViewData["Categories"] = await _categoryService.GetAllWithCountAsync();
        return View(new Product());
    }

    /// <summary>POST /admin/products/create — Lưu sản phẩm mới</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Create(Product product, string? contentBlocksJson, IFormFile? imageFile)
    {
        try
        {
            if (string.IsNullOrEmpty(product.Slug))
                product.Slug = SlugHelper.GenerateSlug(product.Name);

            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "products");
                if (!string.IsNullOrEmpty(url)) product.Image = url;
            }

            if (!string.IsNullOrWhiteSpace(contentBlocksJson))
                product.ContentBlocks = JsonDocument.Parse(contentBlocksJson);

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _productService.CreateAsync(product);
            await _activity.LogAsync("CREATE", "Product", product.Id.ToString(), product.Name);

            TempData["Success"] = $"Đã thêm sản phẩm \"{product.Name}\" thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tạo sản phẩm");
            TempData["Error"] = "Không thể tạo sản phẩm. Vui lòng thử lại.";
        }
        ViewData["Categories"] = await _categoryService.GetAllWithCountAsync();
        return View(product);
    }

    /// <summary>GET /admin/products/edit/{id} — Form chỉnh sửa sản phẩm</summary>
    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();

        ViewData["Title"] = $"Sửa: {product.Name}";
        ViewData["Categories"] = await _categoryService.GetAllWithCountAsync();
        return View(product);
    }

    /// <summary>POST /admin/products/edit/{id} — Lưu chỉnh sửa</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Edit(Guid id, Product product, string? contentBlocksJson, IFormFile? imageFile)
    {
        if (id != product.Id) return BadRequest();

        var existing = await _productService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        try
        {
            existing.Name = product.Name;
            existing.Slug = string.IsNullOrEmpty(product.Slug)
                ? SlugHelper.GenerateSlug(product.Name)
                : product.Slug;
            existing.Sku = product.Sku;
            existing.CategoryId = product.CategoryId;
            existing.Price = product.Price;
            existing.Description = product.Description;
            existing.VideoUrl = product.VideoUrl;
            existing.Status = product.Status;
            existing.IsFeatured = product.IsFeatured;
            existing.MetaTitle = product.MetaTitle;
            existing.MetaDesc = product.MetaDesc;
            existing.UpdatedAt = DateTime.UtcNow;

            // Upload ảnh mới (nếu có) — fallback giữ Image cũ
            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "products");
                if (!string.IsNullOrEmpty(url))
                {
                    _upload.DeleteFile(existing.Image); // xóa ảnh cũ
                    existing.Image = url;
                }
            }
            else if (!string.IsNullOrEmpty(product.Image))
            {
                existing.Image = product.Image;
            }

            if (!string.IsNullOrWhiteSpace(contentBlocksJson))
                existing.ContentBlocks = JsonDocument.Parse(contentBlocksJson);

            await _productService.UpdateAsync(existing);
            await _activity.LogAsync("UPDATE", "Product", existing.Id.ToString(), existing.Name);

            TempData["Success"] = $"Đã cập nhật sản phẩm \"{existing.Name}\"!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            ViewData["Categories"] = await _categoryService.GetAllWithCountAsync();
            return View(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật sản phẩm {Id}", id);
            TempData["Error"] = "Không thể cập nhật sản phẩm. Vui lòng thử lại.";
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>POST /admin/products/delete/{id} — Xóa sản phẩm</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _productService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        try
        {
            _upload.DeleteFile(existing.Image);
            await _productService.DeleteAsync(id);
            await _activity.LogAsync("DELETE", "Product", id.ToString(), existing.Name);
            TempData["Success"] = "Đã xóa sản phẩm!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xóa sản phẩm {Id}", id);
            TempData["Error"] = "Không thể xóa sản phẩm.";
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>POST /admin/products/bulk — Thao tác hàng loạt (ẩn/hiện/xóa)</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk(List<Guid> ids, string action)
    {
        if (ids.Count == 0)
        {
            TempData["Error"] = "Chưa chọn sản phẩm nào!";
            return RedirectToAction(nameof(Index));
        }

        var affected = await _productService.BulkActionAsync(ids, action);
        TempData["Success"] = $"Đã thực hiện '{action}' cho {affected} sản phẩm!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>GET /admin/products/bulkedit — Form chỉnh sửa hàng loạt giá + trạng thái</summary>
    public async Task<IActionResult> BulkEdit(List<Guid> ids)
    {
        if (ids.Count == 0)
        {
            TempData["Error"] = "Chưa chọn sản phẩm nào để chỉnh sửa!";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = $"Chỉnh sửa hàng loạt ({ids.Count} sản phẩm)";
        var products = await _productService.GetByIdsAsync(ids);
        return View(products);
    }

    /// <summary>POST /admin/products/bulkedit — Lưu chỉnh sửa hàng loạt</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkEdit(List<Guid> ids, List<decimal?> prices, List<ProductStatus> statuses)
    {
        if (ids.Count == 0)
        {
            TempData["Error"] = "Không có sản phẩm nào!";
            return RedirectToAction(nameof(Index));
        }

        var affected = await _productService.BulkUpdateAsync(ids, prices, statuses);
        TempData["Success"] = $"Đã cập nhật {affected} sản phẩm!";
        return RedirectToAction(nameof(Index));
    }
}
