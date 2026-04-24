namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Email service — đọc cấu hình SMTP từ SiteSettings, không bao giờ throw.
/// Mọi lỗi đều được log và nuốt để không ảnh hưởng business logic.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email. Luôn trả về true/false, không throw.
    /// Nếu EmailEnabled=false hoặc cấu hình thiếu → return false (không log lỗi).
    /// </summary>
    Task<bool> SendAsync(string to, string subject, string htmlBody, string? textBody = null);

    /// <summary>
    /// Gửi tới NotificationEmail (admin). No-op nếu chưa cấu hình.
    /// </summary>
    Task<bool> NotifyAdminAsync(string subject, string htmlBody);

    /// <summary>
    /// Test connection — dùng cho UI "Send test email".
    /// Returns (success, errorMessage).
    /// </summary>
    Task<(bool Success, string? Error)> TestConnectionAsync(string toEmail);
}
