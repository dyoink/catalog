using AquaCMS.Helpers;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>
/// Admin CRUD sản phẩm — listing, create, edit, delete, bulk action.
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

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Thêm sản phẩm mới";
        ViewData["Categories"] = await _categoryService.GetAllWithCountAsync();
        
        // Khởi tạo các bảng phụ để Tag Helpers hoạt động
        return View(new Product {
            Metadata = new ProductMetadata(),
            Finance = new ProductFinance(),
            Content = new ProductContent(),
            Statistic = new ProductStatistic()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Create(Product product, string? contentBlocksJson, IFormFile? imageFile)
    {
        try
        {
            // Metadata & Slug
            product.Metadata ??= new ProductMetadata();
            if (string.IsNullOrEmpty(product.Metadata.Slug))
                product.Metadata.Slug = SlugHelper.GenerateSlug(product.Name);

            // Image & Content
            product.Content ??= new ProductContent();
            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "products");
                if (!string.IsNullOrEmpty(url)) product.Content.Image = url;
            }

            if (!string.IsNullOrWhiteSpace(contentBlocksJson))
                product.Content.ContentBlocks = JsonDocument.Parse(contentBlocksJson);

            // Statistics & Finance
            product.Statistic ??= new ProductStatistic();
            product.Finance ??= new ProductFinance();

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _productService.CreateAsync(product);
            await _activity.LogAsync("CREATE", "Product", product.Id.ToString(), product.Name);

            TempData["Success"] = $"Đã thêm sản phẩm \"{product.Name}\" thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tạo sản phẩm");
            TempData["Error"] = "Không thể tạo sản phẩm. Vui lòng thử lại.";
        }
        ViewData["Categories"] = await _categoryService.GetAllWithCountAsync();
        return View(product);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();

        // Đảm bảo không bị null
        product.Metadata ??= new ProductMetadata { ProductId = id };
        product.Content ??= new ProductContent { ProductId = id };
        product.Finance ??= new ProductFinance { ProductId = id };
        product.Statistic ??= new ProductStatistic { ProductId = id };

        ViewData["Title"] = $"Sửa: {product.Name}";
        ViewData["Categories"] = await _categoryService.GetAllWithCountAsync();
        return View(product);
    }

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
            // Map Core
            existing.Name = product.Name;
            existing.Sku = product.Sku;
            existing.CategoryId = product.CategoryId;
            existing.Status = product.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            // Map Metadata
            existing.Metadata ??= new ProductMetadata { ProductId = id };
            existing.Metadata.Slug = string.IsNullOrEmpty(product.Metadata?.Slug)
                ? SlugHelper.GenerateSlug(product.Name)
                : product.Metadata.Slug;
            existing.Metadata.MetaTitle = product.Metadata?.MetaTitle;
            existing.Metadata.MetaDesc = product.Metadata?.MetaDesc;

            // Map Finance
            existing.Finance ??= new ProductFinance { ProductId = id };
            existing.Finance.Price = product.Finance?.Price;
            existing.Finance.ShowPrice = product.Finance?.ShowPrice ?? true;
            existing.Finance.IsFeatured = product.Finance?.IsFeatured ?? false;

            // Map Content
            existing.Content ??= new ProductContent { ProductId = id };
            existing.Content.Description = product.Content?.Description;
            existing.Content.VideoUrl = product.Content?.VideoUrl;

            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "products");
                if (!string.IsNullOrEmpty(url))
                {
                    _upload.DeleteFile(existing.Content.Image);
                    existing.Content.Image = url;
                }
            }
            else if (product.Content != null && !string.IsNullOrEmpty(product.Content.Image))
            {
                existing.Content.Image = product.Content.Image;
            }

            if (!string.IsNullOrWhiteSpace(contentBlocksJson))
                existing.Content.ContentBlocks = JsonDocument.Parse(contentBlocksJson);

            await _productService.UpdateAsync(existing);
            await _activity.LogAsync("UPDATE", "Product", existing.Id.ToString(), existing.Name);

            TempData["Success"] = $"Đã cập nhật sản phẩm \"{existing.Name}\"!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật sản phẩm {Id}", id);
            TempData["Error"] = "Không thể cập nhật sản phẩm. Vui lòng thử lại.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _productService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        try
        {
            if (existing.Content != null) _upload.DeleteFile(existing.Content.Image);
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
