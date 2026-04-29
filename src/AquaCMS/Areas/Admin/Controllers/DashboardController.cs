using AquaCMS.Data;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>
/// Admin Dashboard — thống kê tổng quan + analytics nâng cao.
/// Tất cả admin roles đều xem được.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "AnyAdmin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly ISettingsService _settingsService;

    public DashboardController(AppDbContext db, ISettingsService settingsService)
    {
        _db = db;
        _settingsService = settingsService;
    }

    /// <summary>GET /admin — Trang dashboard chính</summary>
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";

        // ===== Thống kê tổng quan =====
        ViewData["TotalProducts"] = await _db.Products.CountAsync();
        ViewData["VisibleProducts"] = await _db.Products
            .CountAsync(p => p.Status != ProductStatus.Hidden);
        ViewData["OutOfStockProducts"] = await _db.Products
            .CountAsync(p => p.Status == ProductStatus.OutOfStock);
        ViewData["TotalPosts"] = await _db.Posts.CountAsync();
        ViewData["PublishedPosts"] = await _db.Posts.CountAsync(p => p.IsPublished);
        ViewData["TotalPartners"] = await _db.Partners.CountAsync(p => p.IsActive);
        ViewData["TotalBanners"] = await _db.Banners.CountAsync(b => b.IsActive);
        ViewData["TotalUsers"] = await _db.Users.CountAsync(u => u.IsActive);
        ViewData["TotalMessages"] = await _db.ChatMessages.CountAsync(m => !m.IsRead);
        ViewData["TotalCategories"] = await _db.Categories.CountAsync();

        // ===== Lượt xem =====
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var monthStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        ViewData["TodayViews"] = await _db.PageViews.CountAsync(v => v.ViewedAt == today);
        ViewData["WeeklyViews"] = await _db.PageViews.CountAsync(v => v.ViewedAt >= weekStart);
        ViewData["MonthlyViews"] = await _db.PageViews.CountAsync(v => v.ViewedAt >= monthStart);

        // Lượt xem 7 ngày gần nhất (cho biểu đồ)
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-6 + i)))
            .ToList();

        var viewsByDay = await _db.PageViews
            .Where(v => v.ViewedAt >= last7Days.First())
            .GroupBy(v => v.ViewedAt)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        ViewData["ChartLabels"] = string.Join(",", last7Days.Select(d => $"\"{d:dd/MM}\""));
        ViewData["ChartData"] = string.Join(",", last7Days.Select(d =>
            viewsByDay.FirstOrDefault(v => v.Date == d)?.Count ?? 0));

        // ===== Sản phẩm xem nhiều nhất =====
        ViewData["TopProducts"] = await _db.Products
            .Where(p => p.Status != ProductStatus.Hidden)
            .OrderByDescending(p => p.Statistic != null ? p.Statistic.ViewCount : 0)
            .Take(5)
            .Select(p => new { 
                p.Name, 
                ViewCount = p.Statistic != null ? p.Statistic.ViewCount : 0, 
                Slug = p.Metadata != null ? p.Metadata.Slug : "", 
                Image = p.Content != null ? p.Content.Image : null, 
                p.ShortId 
            })
            .ToListAsync();

        // ===== Bài viết mới nhất =====
        ViewData["RecentPosts"] = await _db.Posts
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new { p.Title, p.CreatedAt, p.IsPublished, p.Id })
            .ToListAsync();

        // =========================================================
        // ANALYTICS NÂNG CAO (30 ngày gần nhất)
        // =========================================================
        var recentViews = await _db.PageViews
            .Where(v => v.ViewedAt >= monthStart)
            .Select(v => new { v.Path, v.Referrer, v.UserAgent, v.IpAddress })
            .ToListAsync();

        // ----- Top Traffic Sources (theo host của Referrer) -----
        var sourceGroups = recentViews
            .GroupBy(v => ClassifyReferrer(v.Referrer))
            .Select(g => new TrafficSource(g.Key, g.Count()))
            .OrderByDescending(s => s.Count)
            .Take(6)
            .ToList();
        ViewData["TrafficSources"] = sourceGroups;

        // ----- Device Breakdown (Mobile / Tablet / Desktop) -----
        var deviceGroups = recentViews
            .GroupBy(v => ClassifyDevice(v.UserAgent))
            .Select(g => new DeviceStat(g.Key, g.Count()))
            .OrderByDescending(d => d.Count)
            .ToList();
        ViewData["DeviceStats"] = deviceGroups;

        // ----- Top Pages (5 đường dẫn xem nhiều nhất) -----
        var topPaths = recentViews
            .GroupBy(v => v.Path)
            .Select(g => new TopPage(g.Key, g.Count()))
            .OrderByDescending(p => p.Count)
            .Take(8)
            .ToList();
        ViewData["TopPages"] = topPaths;

        // ----- Conversion Rate -----
        var uniqueIps = recentViews
            .Where(v => !string.IsNullOrEmpty(v.IpAddress))
            .Select(v => v.IpAddress!)
            .Distinct()
            .ToHashSet();
        var convertedIps = recentViews
            .Where(v => !string.IsNullOrEmpty(v.IpAddress)
                     && (v.Path.StartsWith("/gio-hang") || v.Path.StartsWith("/dat-hang")))
            .Select(v => v.IpAddress!)
            .Distinct()
            .Count();

        ViewData["UniqueVisitors"] = uniqueIps.Count;
        ViewData["ConvertedVisitors"] = convertedIps;
        ViewData["ConversionRate"] = uniqueIps.Count > 0
            ? Math.Round((double)convertedIps / uniqueIps.Count * 100, 2)
            : 0.0;

        return View();
    }

    private static string ClassifyReferrer(string? referrer)
    {
        if (string.IsNullOrWhiteSpace(referrer)) return "Trực tiếp";
        try
        {
            var host = new Uri(referrer).Host.ToLowerInvariant();
            if (host.Contains("google")) return "Google";
            if (host.Contains("facebook") || host.Contains("fb.com")) return "Facebook";
            if (host.Contains("zalo")) return "Zalo";
            if (host.Contains("youtube")) return "YouTube";
            if (host.Contains("bing")) return "Bing";
            if (host.Contains("instagram")) return "Instagram";
            if (host.Contains("tiktok")) return "TikTok";
            return host.StartsWith("www.") ? host[4..] : host;
        }
        catch
        {
            return "Khác";
        }
    }

    private static string ClassifyDevice(string? ua)
    {
        if (string.IsNullOrWhiteSpace(ua)) return "Khác";
        var u = ua.ToLowerInvariant();
        if (u.Contains("ipad") || u.Contains("tablet")) return "Tablet";
        if (u.Contains("mobi") || u.Contains("android") || u.Contains("iphone")) return "Mobile";
        return "Desktop";
    }

    public record TrafficSource(string Name, int Count);
    public record DeviceStat(string Name, int Count);
    public record TopPage(string Path, int Count);
}
