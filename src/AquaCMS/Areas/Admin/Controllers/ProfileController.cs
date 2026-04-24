using System.Security.Claims;
using AquaCMS.Data;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>
/// Trang profile cá nhân của admin user — đổi mật khẩu, xem thông tin.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "AnyAdmin")]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthService _auth;
    private readonly IActivityLogService _activity;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(AppDbContext db, IAuthService auth, IActivityLogService activity, ILogger<ProfileController> logger)
    {
        _db = db; _auth = auth; _activity = activity; _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Hồ sơ cá nhân";
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ mật khẩu.";
            return RedirectToAction(nameof(Index));
        }

        if (newPassword.Length < 8)
        {
            TempData["Error"] = "Mật khẩu mới phải có ít nhất 8 ký tự.";
            return RedirectToAction(nameof(Index));
        }

        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "Mật khẩu xác nhận không khớp.";
            return RedirectToAction(nameof(Index));
        }

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            if (!_auth.VerifyPassword(currentPassword, user.PasswordHash))
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction(nameof(Index));
            }

            user.PasswordHash = _auth.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _activity.LogAsync("CHANGE_PASSWORD", "User", user.Id.ToString(), "Đổi mật khẩu");
            TempData["Success"] = "Đã đổi mật khẩu thành công!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi đổi mật khẩu user {UserId}", userId);
            TempData["Error"] = "Đã có lỗi khi đổi mật khẩu.";
        }
        return RedirectToAction(nameof(Index));
    }

    private Guid? GetCurrentUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(idStr, out var id) ? id : null;
    }
}
