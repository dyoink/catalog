using System.ComponentModel.DataAnnotations;

namespace AquaCMS.Models.Entities;

/// <summary>
/// Quản lý giá cả và các cờ hiển thị đặc biệt.
/// </summary>
public class ProductFinance
{
    [Key]
    public Guid ProductId { get; set; }

    /// <summary>Giá sản phẩm — NULL nghĩa là "Liên hệ báo giá"</summary>
    public decimal? Price { get; set; }

    /// <summary>Có hiển thị giá ra ngoài hay không</summary>
    public bool ShowPrice { get; set; } = true;

    /// <summary>Đánh dấu sản phẩm nổi bật</summary>
    public bool IsFeatured { get; set; }

    // Navigation
    public virtual Product? Product { get; set; }
}
