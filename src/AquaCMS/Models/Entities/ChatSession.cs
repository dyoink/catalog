namespace AquaCMS.Models.Entities;

/// <summary>
/// Phiên chat khách vãng lai.
/// Mỗi khách được định danh qua GuestId (sinh từ browser).
/// </summary>
public class ChatSession
{
    public Guid Id { get; set; }

    /// <summary>ID khách do browser sinh (ví dụ: "abc-123-xyz")</summary>
    public string GuestId { get; set; } = string.Empty;

    /// <summary>Số tin nhắn chưa đọc (từ phía khách)</summary>
    public int UnreadCount { get; set; }

    /// <summary>Tin nhắn cuối cùng — hiện ở danh sách admin</summary>
    public string? LastMessage { get; set; }

    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation =====
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
