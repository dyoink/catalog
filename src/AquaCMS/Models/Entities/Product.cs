using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaCMS.Models.Entities;

/// <summary>
/// Sản phẩm (Core) — Lưu trữ thông tin định danh cốt lõi.
/// </summary>
public class Product
{
    public Guid Id { get; set; }

    /// <summary>Mã số ngắn auto-increment, dùng cho URL SEO</summary>
    public long ShortId { get; set; }

    /// <summary>Mã sản phẩm nội bộ</summary>
    [MaxLength(100)]
    public string? Sku { get; set; }

    /// <summary>Tên sản phẩm</summary>
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên sản phẩm tối đa 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    /// <summary>FK đến danh mục</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Trạng thái: available / out_of_stock / hidden</summary>
    public ProductStatus Status { get; set; } = ProductStatus.Hidden;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation Properties =====
    public virtual Category? Category { get; set; }
    
    // Relationships (1:1)
    public virtual ProductMetadata? Metadata { get; set; }
    public virtual ProductContent? Content { get; set; }
    public virtual ProductFinance? Finance { get; set; }
    public virtual ProductStatistic? Statistic { get; set; }

    /// <summary>SEO URL helper: /san-pham/slug-shortid</summary>
    public string SeoUrl => Metadata != null ? $"/san-pham/{Metadata.Slug}-{ShortId}" : "#";
}

public enum ProductStatus
{
    Available,
    OutOfStock,
    Hidden
}
