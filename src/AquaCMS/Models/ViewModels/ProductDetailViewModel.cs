using AquaCMS.Models.Entities;

namespace AquaCMS.Models.ViewModels;

/// <summary>
/// ViewModel cho trang chi tiết sản phẩm.
/// </summary>
public class ProductDetailViewModel
{
    /// <summary>Sản phẩm chính</summary>
    public Product Product { get; set; } = null!;

    /// <summary>Sản phẩm liên quan (cùng danh mục)</summary>
    public List<Product> RelatedProducts { get; set; } = new();

    /// <summary>Breadcrumb trail</summary>
    public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
}

/// <summary>
/// Item trong breadcrumb navigation — dùng cho structured data.
/// </summary>
public class BreadcrumbItem
{
    public string Label { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsActive { get; set; }
}
