using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AquaCMS.Models.Entities;

/// <summary>
/// Sản phẩm — entity chính của hệ thống.
/// content_blocks lưu dạng JSONB trong PostgreSQL.
/// </summary>
public class Product
{
    public Guid Id { get; set; }

    /// <summary>Mã số ngắn auto-increment, dùng cho URL SEO: /san-pham/{slug}-{ShortId}</summary>
    public long ShortId { get; set; }

    /// <summary>Slug SEO-friendly (ví dụ: "may-cho-tom-an-360")</summary>
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Tên sản phẩm</summary>
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên sản phẩm tối đa 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Mã sản phẩm nội bộ</summary>
    [MaxLength(100)]
    public string? Sku { get; set; }

    /// <summary>FK đến danh mục — nullable (nếu xóa danh mục → SET NULL)</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Giá sản phẩm — NULL nghĩa là "Liên hệ báo giá"</summary>
    [Range(0, 999_999_999_999, ErrorMessage = "Giá phải >= 0")]
    public decimal? Price { get; set; }

    /// <summary>Mô tả ngắn cho trang listing</summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>URL ảnh chính</summary>
    [MaxLength(500)]
    public string? Image { get; set; }

    /// <summary>URL video (YouTube embed)</summary>
    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    /// <summary>Trạng thái: available / out_of_stock / hidden</summary>
    public ProductStatus Status { get; set; } = ProductStatus.Available;

    /// <summary>
    /// Mảng các khối nội dung động (text, tech-grid, feature, gallery).
    /// Lưu trữ dạng JSONB trong PostgreSQL.
    /// </summary>
    public JsonDocument ContentBlocks { get; set; } = JsonDocument.Parse("[]");

    /// <summary>Đếm lượt xem — tăng tự động khi khách xem detail</summary>
    public int ViewCount { get; set; }

    /// <summary>SEO: Tiêu đề thẻ meta (tối đa 70 ký tự)</summary>
    [MaxLength(70)]
    public string? MetaTitle { get; set; }

    /// <summary>SEO: Mô tả thẻ meta (tối đa 160 ký tự)</summary>
    [MaxLength(160)]
    public string? MetaDesc { get; set; }

    /// <summary>Đánh dấu sản phẩm nổi bật — hiện trên trang chủ</summary>
    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation Properties =====
    public Category? Category { get; set; }

    /// <summary>SEO URL helper: /san-pham/slug-shortid</summary>
    public string SeoUrl => $"/san-pham/{Slug}-{ShortId}";
}

/// <summary>
/// Trạng thái sản phẩm — map với PostgreSQL ENUM 'product_status'.
/// </summary>
public enum ProductStatus
{
    Available,
    OutOfStock,
    Hidden
}
