using AquaCMS.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AquaCMS.Services;

/// <summary>
/// EmailService: dùng MailKit (free, MIT). Đọc cấu hình từ SiteSettings.
/// Cơ chế cô lập lỗi: mọi exception bị nuốt + log, không ảnh hưởng caller.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ISettingsService _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ISettingsService settings, ILogger<EmailService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string to, string subject, string htmlBody, string? textBody = null)
    {
        if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(subject))
        {
            _logger.LogWarning("EmailService.SendAsync: missing 'to' or 'subject', skip.");
            return false;
        }

        try
        {
            var s = await _settings.GetSettingsAsync();
            if (!s.EmailEnabled)
            {
                _logger.LogDebug("EmailService disabled in settings — skip send to {To}", to);
                return false;
            }
            if (string.IsNullOrWhiteSpace(s.SmtpHost) || string.IsNullOrWhiteSpace(s.SmtpFromEmail))
            {
                _logger.LogWarning("EmailService: SMTP host/from not configured. Skip send.");
                return false;
            }

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(s.SmtpFromName ?? s.CompanyName, s.SmtpFromEmail));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody, TextBody = textBody ?? StripHtml(htmlBody) };
            msg.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 15000; // 15s
            var secure = s.SmtpUseSsl
                ? (s.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
                : SecureSocketOptions.None;

            await client.ConnectAsync(s.SmtpHost, s.SmtpPort, secure);
            if (!string.IsNullOrEmpty(s.SmtpUser))
            {
                await client.AuthenticateAsync(s.SmtpUser, s.SmtpPassword ?? "");
            }
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {To} subject={Subject}", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            // KHÔNG throw — email lỗi không được crash app
            _logger.LogError(ex, "EmailService.SendAsync failed to {To}", to);
            return false;
        }
    }

    public async Task<bool> NotifyAdminAsync(string subject, string htmlBody)
    {
        try
        {
            var s = await _settings.GetSettingsAsync();
            var to = !string.IsNullOrWhiteSpace(s.NotificationEmail) ? s.NotificationEmail : s.Email;
            if (string.IsNullOrWhiteSpace(to))
            {
                _logger.LogDebug("NotifyAdminAsync: no recipient configured.");
                return false;
            }
            return await SendAsync(to!, subject, htmlBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NotifyAdminAsync failed");
            return false;
        }
    }

    public async Task<(bool Success, string? Error)> TestConnectionAsync(string toEmail)
    {
        try
        {
            var ok = await SendAsync(toEmail,
                "[AquaCMS] Test email",
                "<p>Đây là email test từ AquaCMS. Nếu bạn nhận được nghĩa là cấu hình SMTP đã đúng.</p>");
            return ok ? (true, null) : (false, "Gửi không thành công — kiểm tra log để biết chi tiết.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";
        try
        {
            return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ").Trim();
        }
        catch { return html; }
    }
}
