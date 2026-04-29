using System.Security.Claims;
using AquaCMS.Models.ViewModels;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AquaCMS.Controllers;

/// <summary>
/// Controller xác thực — đăng nhập/đăng xuất admin.
/// Sử dụng cookie authentication (httpOnly, SameSite=Lax).
/// </summary>
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// GET /dang-nhap — Hiển thị form đăng nhập.
    /// </summary>
    [HttpGet("/dang-nhap")]
    public IActionResult Login(string? returnUrl)
    {
        // ... (giữ nguyên code cũ)
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    /// <summary>
    /// POST /dang-nhap — Xử lý đăng nhập.
    /// </summary>
    [HttpPost("/dang-nhap")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // ... (giữ nguyên code cũ)
        if (!ModelState.IsValid)
            return View(model);

        // Xác thực credentials
        var user = await _authService.ValidateCredentialsAsync(model.Email, model.Password);
        if (user == null)
        {
            model.ErrorMessage = "Email hoặc mật khẩu không đúng";
            return View(model);
        }

        // Tạo claims cho cookie
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        if (!string.IsNullOrEmpty(user.Avatar))
            claims.Add(new Claim("Avatar", user.Avatar));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Tạo auth cookie
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,          // Cookie tồn tại sau khi đóng browser
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            });

        // Cập nhật thời gian đăng nhập cuối
        await _authService.UpdateLastLoginAsync(user.Id);

        // Redirect về trang yêu cầu hoặc admin dashboard
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }

    /// <summary>
    /// GET /dang-xuat — Xóa cookie và redirect về trang chủ.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    /// <summary>GET /khong-co-quyen — Trang thông báo không có quyền truy cập</summary>
    [HttpGet("/khong-co-quyen")]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
