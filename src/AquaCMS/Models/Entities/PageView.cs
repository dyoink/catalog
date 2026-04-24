namespace AquaCMS.Models.Entities;

/// <summary>
/// Ghi nhận lượt xem trang — dùng cho analytics dashboard.
/// Không cần FK vì entity có thể bị xóa mà vẫn muốn giữ page view.
/// </summary>
public class PageView
{
    public long Id { get; set; }

    /// <summary>URL path (ví dụ: "/san-pham/may-cho-tom-360")</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>ID sản phẩm hoặc bài viết (optional)</summary>
    public Guid? EntityId { get; set; }

    /// <summary>Loại entity: "product" hoặc "post"</summary>
    public string? EntityType { get; set; }

    /// <summary>IP client — dùng để rate-limit</summary>
    public string? IpAddress { get; set; }

    /// <summary>User-Agent — phân biệt bot vs người thật, mobile/desktop</summary>
    public string? UserAgent { get; set; }

    /// <summary>Referrer header — dùng để phân tích traffic source (Google, Facebook, direct...)</summary>
    public string? Referrer { get; set; }

    /// <summary>Ngày xem — aggregate theo ngày cho báo cáo</summary>
    public DateOnly ViewedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
