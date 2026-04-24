namespace AquaCMS.Models.Entities;

/// <summary>
/// Log hoạt động của admin — ghi lại mọi thao tác CRUD quan trọng.
/// Dùng để truy vết: ai đã sửa gì, lúc nào, từ IP nào.
/// </summary>
public class ActivityLog
{
    public Guid Id { get; set; }

    /// <summary>FK đến user thực hiện</summary>
    public Guid? UserId { get; set; }

    /// <summary>Tên user (snapshot — phòng khi user bị xóa)</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Hành động: CREATE, UPDATE, DELETE, LOGIN, LOGOUT, BULK_UPDATE...</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Loại entity bị tác động: Product, Post, Partner, User, Settings...</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>ID của entity bị tác động (nếu có)</summary>
    public string? EntityId { get; set; }

    /// <summary>Mô tả chi tiết</summary>
    public string? Description { get; set; }

    /// <summary>IP của request</summary>
    public string? IpAddress { get; set; }

    /// <summary>User-Agent của request</summary>
    public string? UserAgent { get; set; }

    /// <summary>Mức độ: Info (mặc định), Success, Warning, Error.</summary>
    public string Severity { get; set; } = "Info";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
