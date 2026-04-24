namespace AquaCMS.Models.Entities;

/// <summary>
/// Banner trang chủ.
/// Chỉ hiện banner có IsActive = true, sắp xếp theo SortOrder.
/// </summary>
public class Banner
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }

    /// <summary>URL ảnh banner (bắt buộc)</summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>Màu nền (hex hoặc CSS color)</summary>
    public string? Color { get; set; }

    /// <summary>Link khi click vào banner</summary>
    public string? LinkUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
