using System.ComponentModel.DataAnnotations;

namespace AquaCMS.Models.Entities;

/// <summary>
/// Lưu trữ thông tin phục vụ SEO và định danh URL.
/// </summary>
public class ProductMetadata
{
    [Key]
    public Guid ProductId { get; set; }

    /// <summary>Slug SEO-friendly</summary>
    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>SEO: Tiêu đề thẻ meta (tối đa 70 ký tự)</summary>
    [MaxLength(70)]
    public string? MetaTitle { get; set; }

    /// <summary>SEO: Mô tả thẻ meta (tối đa 160 ký tự)</summary>
    [MaxLength(160)]
    public string? MetaDesc { get; set; }

    // Navigation
    public virtual Product? Product { get; set; }
}
