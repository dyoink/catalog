using AquaCMS.Data;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>Admin quản lý người dùng — chỉ SUPER_ADMIN và MANAGER.</summary>
[Area("Admin")]
[Authorize(Policy = "ManagerUp")]
public class UsersController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthService _authService;

    public UsersController(AppDbContext db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Quản lý người dùng";
        var users = await _db.Users
            .OrderBy(u => u.Role)
            .ThenBy(u => u.Name)
            .ToListAsync();
        return View(users);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm người dùng";
        return View(new User());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user, string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            TempData["Error"] = "Mật khẩu phải có ít nhất 8 ký tự!";
            ViewData["Title"] = "Thêm người dùng";
            return View(user);
        }

        // Kiểm tra email trùng
        if (await _db.Users.AnyAsync(u => u.Email == user.Email))
        {
            TempData["Error"] = "Email đã tồn tại!";
            ViewData["Title"] = "Thêm người dùng";
            return View(user);
        }

        user.PasswordHash = _authService.HashPassword(password);
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã thêm người dùng {user.Name}!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Không cho phép tắt chính mình
        var currentUserId = User.FindFirst("UserId")?.Value;
        if (user.Id.ToString() == currentUserId)
        {
            TempData["Error"] = "Không thể vô hiệu hóa chính mình!";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = user.IsActive
            ? $"Đã kích hoạt {user.Name}"
            : $"Đã vô hiệu hóa {user.Name}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Không cho phép xóa chính mình
        var currentUserId = User.FindFirst("UserId")?.Value;
        if (user.Id.ToString() == currentUserId)
        {
            TempData["Error"] = "Không thể xóa chính mình!";
            return RedirectToAction(nameof(Index));
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã xóa {user.Name}!";
        return RedirectToAction(nameof(Index));
    }
}
