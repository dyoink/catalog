using AquaCMS.Models.Entities;

namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service xác thực — cookie-based authentication (không JWT).
/// </summary>
public interface IAuthService
{
    /// <summary>Xác thực email + password, trả về User nếu đúng</summary>
    Task<User?> ValidateCredentialsAsync(string email, string password);

    /// <summary>Hash mật khẩu bằng Argon2id</summary>
    string HashPassword(string password);

    /// <summary>So sánh password với hash</summary>
    bool VerifyPassword(string password, string hash);

    /// <summary>Cập nhật thời gian đăng nhập cuối</summary>
    Task UpdateLastLoginAsync(Guid userId);
}
