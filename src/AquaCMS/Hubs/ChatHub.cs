using AquaCMS.Data;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Hubs;

/// <summary>
/// SignalR hub cho chat realtime giữa khách hàng (guest) và admin.
/// Không yêu cầu auth cho guest — admin có thể là anonymous on this hub
/// nhưng các action gửi từ admin được tag IsFromAdmin=true ở client.
/// Mọi method bọc try/catch + log để không bao giờ kill connection.
/// </summary>
public class ChatHub : Hub
{
    private const string AdminGroup = "admins";

    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(AppDbContext db, IEmailService email, ILogger<ChatHub> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    /// <summary>Guest tham gia phiên chat của chính mình (group = guestId).</summary>
    public async Task JoinAsGuest(string guestId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(guestId)) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, GuestGroup(guestId));
        }
        catch (Exception ex) { _logger.LogError(ex, "JoinAsGuest failed"); }
    }

    /// <summary>Admin join group "admins" để nhận tất cả tin từ khách.</summary>
    public async Task JoinAsAdmin()
    {
        try
        {
            // Best-effort: admin auth check via Context.User
            if (Context.User?.Identity?.IsAuthenticated != true) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        }
        catch (Exception ex) { _logger.LogError(ex, "JoinAsAdmin failed"); }
    }

    /// <summary>Guest gửi tin nhắn — tạo session nếu chưa có, broadcast cho admin.</summary>
    public async Task SendFromGuest(string guestId, string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(guestId) || string.IsNullOrWhiteSpace(text)) return;
            text = text.Trim();
            if (text.Length > 4000) text = text[..4000];

            var session = await _db.ChatSessions.FirstOrDefaultAsync(c => c.GuestId == guestId);
            if (session == null)
            {
                session = new ChatSession
                {
                    Id = Guid.NewGuid(),
                    GuestId = guestId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ChatSessions.Add(session);
            }

            var msg = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                SenderId = guestId,
                IsFromAdmin = false,
                Text = text,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.ChatMessages.Add(msg);

            session.LastMessage = text;
            session.UpdatedAt = DateTime.UtcNow;
            session.UnreadCount += 1;

            await _db.SaveChangesAsync();

            var payload = new { session = session.Id, guestId, text, isFromAdmin = false, at = msg.CreatedAt };
            await Clients.Group(GuestGroup(guestId)).SendAsync("ReceiveMessage", payload);
            await Clients.Group(AdminGroup).SendAsync("ReceiveMessage", payload);

            // Notify admin via email (fire-and-forget, isolated)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _email.NotifyAdminAsync(
                        $"[Chat] Tin nhắn mới từ khách {guestId}",
                        $"<p><strong>{System.Net.WebUtility.HtmlEncode(guestId)}:</strong></p><p>{System.Net.WebUtility.HtmlEncode(text)}</p>");
                }
                catch (Exception ex) { _logger.LogError(ex, "Email notify failed"); }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendFromGuest failed for {GuestId}", guestId);
        }
    }

    /// <summary>Admin trả lời guest cụ thể.</summary>
    public async Task SendFromAdmin(string guestId, string text)
    {
        try
        {
            if (Context.User?.Identity?.IsAuthenticated != true) return;
            if (string.IsNullOrWhiteSpace(guestId) || string.IsNullOrWhiteSpace(text)) return;
            text = text.Trim();
            if (text.Length > 4000) text = text[..4000];

            var session = await _db.ChatSessions.FirstOrDefaultAsync(c => c.GuestId == guestId);
            if (session == null) return;

            var msg = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                SenderId = "admin",
                IsFromAdmin = true,
                Text = text,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.ChatMessages.Add(msg);

            session.LastMessage = text;
            session.UpdatedAt = DateTime.UtcNow;
            // admin reply resets unread (admin has read this thread)
            session.UnreadCount = 0;

            await _db.SaveChangesAsync();

            var payload = new { session = session.Id, guestId, text, isFromAdmin = true, at = msg.CreatedAt };
            await Clients.Group(GuestGroup(guestId)).SendAsync("ReceiveMessage", payload);
            await Clients.Group(AdminGroup).SendAsync("ReceiveMessage", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendFromAdmin failed");
        }
    }

    private static string GuestGroup(string guestId) => $"guest:{guestId}";
}
