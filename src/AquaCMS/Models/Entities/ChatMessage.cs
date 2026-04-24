namespace AquaCMS.Models.Entities;

/// <summary>
/// Tin nhắn chat đơn lẻ trong một phiên.
/// </summary>
public class ChatMessage
{
    public Guid Id { get; set; }

    /// <summary>FK đến ChatSession</summary>
    public Guid SessionId { get; set; }

    /// <summary>ID người gửi: guestId hoặc "admin"</summary>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>Tin nhắn từ admin hay khách</summary>
    public bool IsFromAdmin { get; set; }

    /// <summary>Nội dung tin nhắn</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Admin đã đọc chưa</summary>
    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation =====
    public ChatSession? Session { get; set; }
}
