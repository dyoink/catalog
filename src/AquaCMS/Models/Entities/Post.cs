namespace AquaCMS.Models.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Bài viết kiến thức / blog.
/// Chỉ hiện cho khách khi IsPublished = true.
/// </summary>
public class Post
{
    public Guid Id { get; set; }

    /// <summary>Mã số ngắn auto-increment, dùng cho URL SEO: /kien-thuc/{slug}-{ShortId}</summary>
    public long ShortId { get; set; }

    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Tóm tắt ngắn hiện ở listing</summary>
    [MaxLength(500)]
    public string? Excerpt { get; set; }

    /// <summary>Nội dung HTML đầy đủ</summary>
    [Required(ErrorMessage = "Nội dung không được để trống")]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Image { get; set; }

    [MaxLength(100)]
    public string Author { get; set; } = "Admin";

    public Guid? KnowledgeCategoryId { get; set; }

    /// <summary>Thời gian đọc ước tính (ví dụ: "8 phút")</summary>
    [MaxLength(50)]
    public string? ReadTime { get; set; }

    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }

    // SEO
    [MaxLength(70)]
    public string? MetaTitle { get; set; }
    [MaxLength(160)]
    public string? MetaDesc { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation =====
    public KnowledgeCategory? KnowledgeCategory { get; set; }

    public string SeoUrl => $"/kien-thuc/{Slug}-{ShortId}";
}
