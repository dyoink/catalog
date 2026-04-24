using AquaCMS.Data;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Modules.Core.Middleware;

/// <summary>
/// Middleware ghi nhận page view cho analytics dashboard.
/// Bỏ qua: admin, API, static files, asset paths, bots phổ biến.
/// Dedup bằng in-memory cache theo IP + path trong vòng 30 phút để tránh inflate số liệu.
/// </summary>
public class PageViewTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PageViewTrackingMiddleware> _logger;

    // Dedup nhẹ — IP|path → DateTime; xoá entries cũ định kỳ
    private static readonly Dictionary<string, DateTime> _seen = new();
    private static readonly object _lock = new();
    private static DateTime _lastSweep = DateTime.UtcNow;
    private static readonly TimeSpan _dedupWindow = TimeSpan.FromMinutes(30);

    private static readonly string[] _skipPrefixes = new[]
    {
        "/admin", "/api/", "/hubs/", "/uploads/", "/css/", "/js/", "/images/", "/lib/",
        "/dang-nhap", "/dang-xuat", "/khong-co-quyen", "/loi/",
        "/sitemap.xml", "/robots.txt", "/favicon.ico"
    };

    private static readonly string[] _botFragments = new[]
    {
        "bot", "spider", "crawler", "slurp", "facebookexternalhit", "embed",
        "headlesschrome", "preview", "monitor", "uptime", "lighthouse"
    };

    public PageViewTrackingMiddleware(RequestDelegate next, ILogger<PageViewTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        await _next(ctx);

        try
        {
            // Chỉ track GET 200 OK trên trang HTML public
            if (ctx.Request.Method != "GET") return;
            if (ctx.Response.StatusCode != StatusCodes.Status200OK) return;

            var path = ctx.Request.Path.Value ?? "/";
            if (string.IsNullOrEmpty(path)) return;
            foreach (var prefix in _skipPrefixes)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;
            }

            // Skip nếu có file extension (asset)
            if (Path.HasExtension(path) && !path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                return;

            var ua = ctx.Request.Headers.UserAgent.ToString() ?? "";
            var uaLower = ua.ToLowerInvariant();
            foreach (var bot in _botFragments)
            {
                if (uaLower.Contains(bot)) return;
            }

            var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var dedupKey = ip + "|" + path;

            lock (_lock)
            {
                // Sweep entries cũ mỗi 5 phút
                if (DateTime.UtcNow - _lastSweep > TimeSpan.FromMinutes(5))
                {
                    var cutoff = DateTime.UtcNow - _dedupWindow;
                    var stale = _seen.Where(kv => kv.Value < cutoff).Select(kv => kv.Key).ToList();
                    foreach (var k in stale) _seen.Remove(k);
                    _lastSweep = DateTime.UtcNow;
                }

                if (_seen.TryGetValue(dedupKey, out var last)
                    && DateTime.UtcNow - last < _dedupWindow)
                    return;

                _seen[dedupKey] = DateTime.UtcNow;
            }

            var referrer = ctx.Request.Headers.Referer.ToString();
            if (referrer.Length > 500) referrer = referrer[..500];
            if (ua.Length > 500) ua = ua[..500];
            if (path.Length > 500) path = path[..500];

            // Detect entity type theo URL pattern
            string? entityType = null;
            if (path.StartsWith("/san-pham/", StringComparison.OrdinalIgnoreCase)) entityType = "product";
            else if (path.StartsWith("/kien-thuc/", StringComparison.OrdinalIgnoreCase)) entityType = "post";
            else if (path.StartsWith("/doi-tac/", StringComparison.OrdinalIgnoreCase)) entityType = "partner";

            // TẠO SCOPE RIÊNG ĐỂ TRÁNH CONCURRENCY LỖI DB CONTEXT
            using (var scope = ctx.RequestServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.PageViews.Add(new Models.Entities.PageView
                {
                    Path = path,
                    EntityType = entityType,
                    IpAddress = ip,
                    UserAgent = string.IsNullOrEmpty(ua) ? null : ua,
                    Referrer = string.IsNullOrEmpty(referrer) ? null : referrer,
                    ViewedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                });
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PageView tracking failed for {Path}", ctx.Request.Path);
        }
    }
}

public static class PageViewTrackingMiddlewareExtensions
{
    public static IApplicationBuilder UsePageViewTracking(this IApplicationBuilder app)
        => app.UseMiddleware<PageViewTrackingMiddleware>();
}
