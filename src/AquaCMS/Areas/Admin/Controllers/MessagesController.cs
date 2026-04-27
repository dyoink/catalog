using AquaCMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>
/// Trang quản lý tin nhắn chat từ khách. Hiển thị danh sách session + chi tiết.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "AnyAdmin")]
public class MessagesController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(AppDbContext db, ILogger<MessagesController> logger)
    {
        _db = db; _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Tin nhắn khách hàng";
        try
        {
            var sessions = await _db.ChatSessions
                .AsNoTracking()
                .OrderByDescending(s => s.UpdatedAt)
                .Take(100)
                .ToListAsync();
            return View(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi load chat sessions");
            return View(new List<Models.Entities.ChatSession>());
        }
    }

    /// <summary>GET /admin/messages/detail/{id} — Xem messages của 1 session</summary>
    public async Task<IActionResult> Detail(Guid id)
    {
        var session = await _db.ChatSessions
            .AsNoTracking()
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null) return NotFound();

        ViewData["Title"] = $"Chat: {session.GuestId}";

        // Reset unread count và đánh dấu tất cả message đã đọc
        try
        {
            await _db.ChatSessions
                .Where(s => s.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.UnreadCount, 0));

            await _db.ChatMessages
                .Where(m => m.SessionId == id && !m.IsRead && !m.IsFromAdmin)
                .ExecuteUpdateAsync(m => m.SetProperty(x => x.IsRead, true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi reset unread count/messages cho session {Id}", id);
        }

        return View(session);
    }
}
