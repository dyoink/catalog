using AquaCMS.Data;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Hubs;

/// <summary>
/// SignalR hub cho chat realtime giữa khách hàng (guest) và admin.
/// </summary>
public class ChatHub : Hub
{
    private const string AdminGroup = "admins";

    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(AppDbContext db, IEmailService email, ISettingsService settingsService, ILogger<ChatHub> logger)
    {
        _db = db;
        _email = email;
        _settingsService = settingsService;
        _logger = logger;
    }

    private static string GuestGroup(string guestId) => $"guest:{guestId}";

    /// <summary>Guest tham gia phiên chat (group = guestId).</summary>
    public async Task JoinAsGuest(string guestId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(guestId)) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, GuestGroup(guestId));
            
            // Yêu cầu của bạn: Không hiện lại tin nhắn cũ khi khách F5/kết nối lại
            await Clients.Caller.SendAsync("ReceiveHistory", new List<object>());
        }
        catch (Exception ex) { _logger.LogError(ex, "JoinAsGuest failed"); }
    }

    /// <summary>Admin join group "admins" để nhận tất cả tin từ khách.</summary>
    public async Task JoinAsAdmin()
    {
        try
        {
            if (Context.User?.Identity?.IsAuthenticated != true) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        }
        catch (Exception ex) { _logger.LogError(ex, "JoinAsAdmin failed"); }
    }

    /// <summary>Guest gửi tin nhắn — broadcast cho admin và khách.</summary>
    public async Task SendFromGuest(string guestId, string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(guestId) || string.IsNullOrWhiteSpace(text)) return;
            text = text.Trim();
            if (text.Length > 4000) text = text[..4000];

            var shouldAutoReply = false;
            var session = await _db.ChatSessions.FirstOrDefaultAsync(c => c.GuestId == guestId);
            
            if (session == null)
            {
                shouldAutoReply = true;
                session = new ChatSession
                {
                    Id = Guid.NewGuid(),
                    GuestId = guestId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ChatSessions.Add(session);
            }
            else
            {
                // Nếu khách quay lại nhắn tin sau hơn 10 phút kể từ tin nhắn cuối
                if ((DateTime.UtcNow - session.UpdatedAt).TotalMinutes >= 10)
                {
                    shouldAutoReply = true;
                }
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

            // Notify admin qua email
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

            // Gửi tin nhắn tự động (Auto Reply)
            if (shouldAutoReply)
            {
                var settings = await _settingsService.GetSettingsAsync();
                if (!string.IsNullOrWhiteSpace(settings.ChatAutoReplyMessage))
                {
                    await Task.Delay(1500);

                    var replyMsg = new ChatMessage
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        SenderId = "admin",
                        IsFromAdmin = true,
                        Text = settings.ChatAutoReplyMessage,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.ChatMessages.Add(replyMsg);
                    
                    session.LastMessage = settings.ChatAutoReplyMessage;
                    session.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    var replyPayload = new { session = session.Id, guestId, text = settings.ChatAutoReplyMessage, isFromAdmin = true, at = replyMsg.CreatedAt };
                    await Clients.Group(GuestGroup(guestId)).SendAsync("ReceiveMessage", replyPayload);
                    await Clients.Group(AdminGroup).SendAsync("ReceiveMessage", replyPayload);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendFromGuest failed for {GuestId}", guestId);
        }
    }

    /// <summary>Admin trả lời guest.</summary>
    public async Task SendFromAdmin(string guestId, string text)
    {
        try
        {
            if (Context.User?.Identity?.IsAuthenticated != true) return;
            if (string.IsNullOrWhiteSpace(guestId) || string.IsNullOrWhiteSpace(text)) return;
            text = text.Trim();

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
}
