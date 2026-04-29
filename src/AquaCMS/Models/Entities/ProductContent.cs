using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AquaCMS.Models.Entities;

/// <summary>
/// Lưu trữ các nội dung nặng về dung lượng.
/// </summary>
public class ProductContent
{
    [Key]
    public Guid ProductId { get; set; }

    /// <summary>Mô tả ngắn cho trang listing</summary>
    public string? Description { get; set; }

    /// <summary>Nội dung chi tiết dạng khối (JSONB)</summary>
    public JsonDocument ContentBlocks { get; set; } = JsonDocument.Parse("[]");

    /// <summary>URL ảnh chính</summary>
    [MaxLength(500)]
    public string? Image { get; set; }

    /// <summary>URL video (YouTube embed)</summary>
    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    // Navigation
    public virtual Product? Product { get; set; }
}
