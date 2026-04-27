using AquaCMS.Data;
using AquaCMS.Models.Entities;
using AquaCMS.Models.ViewModels;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Controllers;

/// <summary>
/// Controller trang chủ — hiển thị banners, danh mục, sản phẩm nổi bật, bài viết mới.
/// SEO: Trang chủ có đầy đủ structured data Organization.
/// </summary>
public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ISettingsService _settingsService;
    private readonly AppDbContext _db;

    public HomeController(
        IProductService productService,
        ICategoryService categoryService,
        ISettingsService settingsService,
        AppDbContext db)
    {
        _productService = productService;
        _categoryService = categoryService;
        _settingsService = settingsService;
        _db = db;
    }

    /// <summary>
    /// GET / — Trang chủ.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetSettingsAsync();

        var model = new HomeViewModel
        {
            Settings = settings,

            // Chỉ query khi module được bật trong CMS settings
            Banners = settings.ShowBanners
                ? await _db.Banners.Where(b => b.IsActive).OrderBy(b => b.SortOrder).ToListAsync()
                : new(),

            Categories = settings.ShowCategories
                ? await _categoryService.GetAllWithCountAsync()
                : new(),

            FeaturedProducts = settings.ShowFeaturedProducts
                ? await _productService.GetFeaturedProductsAsync(settings.FeaturedProductsCount)
                : new(),

            LatestPosts = settings.ShowLatestPosts
                ? await _db.Posts.Where(p => p.IsPublished).OrderByDescending(p => p.PublishedAt)
                    .Take(settings.LatestPostsCount).ToListAsync()
                : new()
        };

        // SEO meta tags — truyền qua ViewData
        ViewData["Title"] = settings.CompanyName;
        ViewData["MetaDescription"] = $"{settings.CompanyName} — Giải pháp thiết bị nuôi trồng thủy sản hàng đầu";
        ViewData["OgType"] = "website";

        return View(model);
    }

    /// <summary>
    /// GET /loi/{code} — Trang lỗi (404, 500...) hoặc /Home/Error fallback.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? code = null)
    {
        var statusCode = code ?? 500;
        Response.StatusCode = statusCode;
        ViewBag.StatusCode = statusCode;
        ViewBag.Title = statusCode switch
        {
            404 => "Không tìm thấy trang",
            403 => "Không có quyền truy cập",
            500 => "Lỗi máy chủ",
            _ => "Đã có lỗi xảy ra"
        };
        ViewBag.Message = statusCode switch
        {
            404 => "Trang bạn truy cập không tồn tại hoặc đã bị xóa.",
            403 => "Bạn không có quyền truy cập trang này.",
            500 => "Máy chủ gặp sự cố. Vui lòng thử lại sau.",
            _ => "Đã có lỗi không xác định."
        };
        return View();
    }
}
