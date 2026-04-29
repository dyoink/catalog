using AquaCMS.Models.ViewModels;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Controllers;

/// <summary>
/// Controller sản phẩm — listing, detail, category filter.
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

        ViewData["Title"] = $"{model.PageTitle} | {settings.CompanyName}";
        ViewData["MetaDescription"] = $"Danh sách sản phẩm thiết bị nuôi trồng thủy sản — {settings.CompanyName}";

        return View(model);
    }

    public async Task<IActionResult> Category(string slug, int page = 1)
    {
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

        ViewData["Title"] = $"{category.Name} | {settings.CompanyName}";
        ViewData["MetaDescription"] = $"Sản phẩm {category.Name} — {settings.CompanyName}";

        return View("Index", model);
    }

    public async Task<IActionResult> Detail(string slugAndId)
    {
        if (string.IsNullOrWhiteSpace(slugAndId))
            return NotFound();

        var (urlSlug, shortId) = ParseSlugAndId(slugAndId);
        if (shortId == null)
        {
            var byOldSlug = await _productService.GetBySlugAsync(slugAndId);
            if (byOldSlug == null || byOldSlug.Status == Models.Entities.ProductStatus.Hidden || byOldSlug.Metadata == null)
                return NotFound();
            
            return RedirectPermanent($"/san-pham/{byOldSlug.Metadata.Slug}-{byOldSlug.ShortId}");
        }

        var product = await _productService.GetByShortIdAsync(shortId.Value);

        if (product == null || product.Status == Models.Entities.ProductStatus.Hidden || product.Metadata == null)
            return NotFound();

        if (!string.Equals(product.Metadata.Slug, urlSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectPermanent($"/san-pham/{product.Metadata.Slug}-{product.ShortId}");
        }

        var settings = await _settingsService.GetSettingsAsync();
        await _productService.IncrementViewCountAsync(product.Id);

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

        ViewData["Title"] = product.Metadata.MetaTitle ?? $"{product.Name} | {settings.CompanyName}";
        ViewData["MetaDescription"] = product.Metadata.MetaDesc ?? product.Content?.Description ?? "";
        ViewData["OgImage"] = product.Content?.Image;
        ViewData["OgType"] = "product";
        ViewData["CanonicalUrl"] = $"/san-pham/{product.Metadata.Slug}-{product.ShortId}";

        return View(model);
    }

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
