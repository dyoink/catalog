using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;

namespace AquaCMS.Models.ViewModels;

/// <summary>
/// ViewModel cho trang danh sách sản phẩm (bao gồm cả trang danh mục).
/// </summary>
public class ProductListViewModel
{
    /// <summary>Danh sách sản phẩm phân trang</summary>
    public PaginatedList<Product> Products { get; set; } = null!;

    /// <summary>Tất cả danh mục — dùng cho sidebar filter</summary>
    public List<Category> Categories { get; set; } = new();

    /// <summary>Danh mục đang được lọc (null = tất cả)</summary>
    public Category? CurrentCategory { get; set; }

    /// <summary>Từ khóa tìm kiếm hiện tại</summary>
    public string? SearchQuery { get; set; }

    /// <summary>Tiêu đề trang (SEO)</summary>
    public string PageTitle { get; set; } = "Sản phẩm";
}
