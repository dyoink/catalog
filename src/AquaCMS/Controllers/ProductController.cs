using AquaCMS.Models.ViewModels;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Controllers;

/// <summary>
/// Controller sản phẩm — listing, detail, category filter.
/// URL scheme: /san-pham, /san-pham/{slug}, /danh-muc/{slug}
/// </summary>
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ISettingsService _settingsService;

    public ProductController(
        IProductService productService,
        ICategoryService categoryService,
        ISettingsService settingsService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _settingsService = settingsService;
    }

    /// <summary>
    /// GET /san-pham — Danh sách tất cả sản phẩm (có search + phân trang).
    /// </summary>
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var categories = await _categoryService.GetAllWithCountAsync();
        var products = await _productService.GetPublicProductsAsync(
            search: search, page: page, pageSize: 12);

        var model = new ProductListViewModel
        {
            Products = products,
            Categories = categories,
            SearchQuery = search,
            PageTitle = string.IsNullOrWhiteSpace(search)
                ? "Sản phẩm"
                : $"Kết quả tìm kiếm: {search}"
        };

        // SEO
        ViewData["Title"] = $"{model.PageTitle} | {settings.CompanyName}";
        ViewData["MetaDescription"] = $"Danh sách sản phẩm thiết bị nuôi trồng thủy sản — {settings.CompanyName}";
        ViewData["CanonicalUrl"] = Url.RouteUrl("product-list");

        return View(model);
    }

    /// <summary>
    /// GET /danh-muc/{slug} — Sản phẩm theo danh mục.
    /// </summary>
    public async Task<IActionResult> Category(string slug, int page = 1)
    {
        // Tìm danh mục theo slug
        var category = await _categoryService.GetBySlugAsync(slug);
        if (category == null)
            return NotFound();

        var settings = await _settingsService.GetSettingsAsync();
        var categories = await _categoryService.GetAllWithCountAsync();
        var products = await _productService.GetPublicProductsAsync(
            categorySlug: slug, page: page, pageSize: 12);

        var model = new ProductListViewModel
        {
            Products = products,
            Categories = categories,
            CurrentCategory = category,
            PageTitle = category.Name
        };

        // SEO
        ViewData["Title"] = $"{category.Name} | {settings.CompanyName}";
        ViewData["MetaDescription"] = $"Sản phẩm {category.Name} — {settings.CompanyName}";
        ViewData["CanonicalUrl"] = Url.RouteUrl("category-products", new { slug });

        return View("Index", model); // Reuse cùng view
    }

    /// <summary>
    /// GET /san-pham/{slugAndId} — Chi tiết sản phẩm.
    /// URL pattern: /san-pham/may-cho-tom-an-12345
    /// - Trích shortId ở cuối URL (sau dấu - cuối cùng)
    /// - Nếu slug trong URL khác slug DB → 301 redirect (SEO best practice)
    /// - Tăng ViewCount +1, JSON-LD Product structured data.
    /// </summary>
    public async Task<IActionResult> Detail(string slugAndId)
    {
        if (string.IsNullOrWhiteSpace(slugAndId))
            return NotFound();

        // Parse: tách shortId (số ở cuối) khỏi slug
        var (urlSlug, shortId) = ParseSlugAndId(slugAndId);
        if (shortId == null)
        {
            // Fallback: thử coi toàn bộ là slug (backward-compat URL cũ)
            var byOldSlug = await _productService.GetBySlugAsync(slugAndId);
            if (byOldSlug == null || byOldSlug.Status == Models.Entities.ProductStatus.Hidden)
                return NotFound();
            // 301 redirect lên URL mới có shortId
            return RedirectPermanent($"/san-pham/{byOldSlug.Slug}-{byOldSlug.ShortId}");
        }

        var product = await _productService.GetByShortIdAsync(shortId.Value);

        if (product == null || product.Status == Models.Entities.ProductStatus.Hidden)
            return NotFound();

        // Nếu slug khác (vd: admin đổi tên sản phẩm) → 301 redirect lên URL canonical
        if (!string.Equals(product.Slug, urlSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectPermanent($"/san-pham/{product.Slug}-{product.ShortId}");
        }

        var settings = await _settingsService.GetSettingsAsync();

        // Tăng lượt xem (fire-and-forget, không block response)
        _ = _productService.IncrementViewCountAsync(product.Id);

        var related = await _productService.GetRelatedProductsAsync(
            product.Id, product.CategoryId, count: 4);

        var model = new ProductDetailViewModel
        {
            Product = product,
            RelatedProducts = related,
            Breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Label = "Trang chủ", Url = "/" },
                new() { Label = "Sản phẩm", Url = "/san-pham" },
            }
        };

        if (product.Category != null)
        {
            model.Breadcrumbs.Add(new BreadcrumbItem
            {
                Label = product.Category.Name,
                Url = $"/danh-muc/{product.Category.Slug}"
            });
        }

        model.Breadcrumbs.Add(new BreadcrumbItem
        {
            Label = product.Name,
            IsActive = true
        });

        // SEO
        ViewData["Title"] = product.MetaTitle ?? $"{product.Name} | {settings.CompanyName}";
        ViewData["MetaDescription"] = product.MetaDesc ?? product.Description ?? "";
        ViewData["OgImage"] = product.Image;
        ViewData["OgType"] = "product";
        ViewData["CanonicalUrl"] = $"/san-pham/{product.Slug}-{product.ShortId}";

        return View(model);
    }

    /// <summary>
    /// Tách "may-cho-tom-an-12345" thành ("may-cho-tom-an", 12345).
    /// </summary>
    private static (string slug, long? shortId) ParseSlugAndId(string slugAndId)
    {
        var lastDash = slugAndId.LastIndexOf('-');
        if (lastDash <= 0 || lastDash == slugAndId.Length - 1)
            return (slugAndId, null);

        var idPart = slugAndId[(lastDash + 1)..];
        if (long.TryParse(idPart, out var id))
            return (slugAndId[..lastDash], id);

        return (slugAndId, null);
    }
}
