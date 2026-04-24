using AquaCMS.Data;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Services;

/// <summary>
/// Implementation AuthService — xác thực bằng cookie, hash password Argon2id.
/// Không dùng JWT — session-based authentication an toàn hơn cho MVC.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext db, ILogger<AuthService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        // Tìm user theo email (case-insensitive)
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            _logger.LogWarning("Đăng nhập thất bại — email không tồn tại: {Email}", email);
            return null;
        }

        // Kiểm tra tài khoản bị khóa
        if (!user.IsActive)
        {
            _logger.LogWarning("Đăng nhập thất bại — tài khoản bị khóa: {Email}", email);
            return null;
        }

        // So sánh password với Argon2id hash
        if (!VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning("Đăng nhập thất bại — sai mật khẩu: {Email}", email);
            return null;
        }

        _logger.LogInformation("Đăng nhập thành công: {Email} (Role: {Role})", email, user.Role);
        return user;
    }

    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        // Argon2id — OWASP recommended (2025+)
        return Argon2.Hash(password);
    }

    /// <inheritdoc/>
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return Argon2.Verify(hash, password);
        }
        catch (Exception ex)
        {
            // Hash format không hợp lệ — có thể do seed data placeholder
            _logger.LogError(ex, "Lỗi verify password hash");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateLastLoginAsync(Guid userId)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE users SET last_login_at = NOW() WHERE id = {userId}");
    }
}
