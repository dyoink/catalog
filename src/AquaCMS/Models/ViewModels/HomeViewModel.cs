using AquaCMS.Models.Entities;

namespace AquaCMS.Models.ViewModels;

/// <summary>
/// ViewModel cho trang chủ — tổng hợp tất cả data cần render.
/// </summary>
public class HomeViewModel
{
    /// <summary>Danh sách banner hoạt động</summary>
    public List<Banner> Banners { get; set; } = new();

    /// <summary>Danh sách danh mục sản phẩm</summary>
    public List<Category> Categories { get; set; } = new();

    /// <summary>Sản phẩm nổi bật</summary>
    public List<Product> FeaturedProducts { get; set; } = new();

    /// <summary>Bài viết mới nhất</summary>
    public List<Post> LatestPosts { get; set; } = new();

    /// <summary>Settings chung (company name, colors...)</summary>
    public SiteSettings Settings { get; set; } = new();
}
