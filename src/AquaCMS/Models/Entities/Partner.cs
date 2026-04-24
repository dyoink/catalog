namespace AquaCMS.Models.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Đối tác kinh doanh.
/// Chỉ hiện cho khách khi IsActive = true.
/// </summary>
public class Partner
{
    public Guid Id { get; set; }

    /// <summary>Mã số ngắn auto-increment, dùng cho URL SEO: /doi-tac/{slug}-{ShortId}</summary>
    public long ShortId { get; set; }

    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên đối tác không được để trống")]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Mô tả ngắn</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Mô tả chi tiết dạng HTML</summary>
    public string? DetailedDescription { get; set; }

    public Guid? PartnerCategoryId { get; set; }

    /// <summary>Địa điểm (ví dụ: "Cà Mau")</summary>
    [MaxLength(100)]
    public string? Location { get; set; }

    /// <summary>Năm hợp tác (ví dụ: "2022")</summary>
    [MaxLength(20)]
    public string? Since { get; set; }

    [MaxLength(500)]
    public string? Image { get; set; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [MaxLength(255)]
    public string? ContactEmail { get; set; }

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    [Url(ErrorMessage = "URL website không hợp lệ")]
    [MaxLength(500)]
    public string? Website { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation =====
    public PartnerCategory? PartnerCategory { get; set; }

    public string SeoUrl => $"/doi-tac/{Slug}-{ShortId}";
}
