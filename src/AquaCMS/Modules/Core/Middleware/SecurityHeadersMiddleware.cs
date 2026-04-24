namespace AquaCMS.Modules.Core.Middleware;

/// <summary>
/// Thêm các HTTP security headers vào mọi response.
/// Chống XSS, clickjacking, MIME sniffing, referrer leak.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Chống clickjacking — không cho embed trong iframe ngoài
        headers["X-Frame-Options"] = "SAMEORIGIN";

        // Chống MIME-type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // XSS protection (legacy browsers)
        headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer policy — gửi origin khi cross-origin
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions policy — tắt camera, micro, geolocation trừ khi cần
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // Content-Security-Policy — cho phép Tailwind CDN, Lucide, HTMX, Alpine.js
        headers["Content-Security-Policy"] = string.Join("; ",
            "default-src 'self'",
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.tailwindcss.com https://unpkg.com https://cdn.jsdelivr.net",
            "style-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com https://fonts.googleapis.com https://cdn.jsdelivr.net",
            "font-src 'self' https://fonts.gstatic.com",
            "img-src 'self' data: https:",
            "connect-src 'self' ws: wss: https://unpkg.com https://cdn.jsdelivr.net",
            "frame-ancestors 'self'"
        );

        await _next(context);
    }
}

/// <summary>
/// Extension method để đăng ký middleware trong Program.cs
/// </summary>
public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
