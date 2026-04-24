using AquaCMS.Data;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AquaCMS.Services;

/// <summary>
/// Implementation SettingsService — cache settings trong memory.
/// Mỗi request không cần query DB, chỉ đọc từ cache.
/// Cache bị xóa khi admin cập nhật settings.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SettingsService> _logger;
    private const string CacheKey = "site_settings";

    public SettingsService(AppDbContext db, IMemoryCache cache, ILogger<SettingsService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SiteSettings> GetSettingsAsync()
    {
        // Thử lấy từ cache trước
        if (_cache.TryGetValue(CacheKey, out SiteSettings? cached) && cached != null)
            return cached;

        // Cache miss — query DB
        var settings = await _db.SiteSettings.FirstOrDefaultAsync();

        // Nếu chưa có settings nào → tạo mới với giá trị mặc định
        if (settings == null)
        {
            settings = new SiteSettings();
            _db.SiteSettings.Add(settings);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Tạo SiteSettings mặc định");
        }

        // Lưu vào cache 5 phút
        _cache.Set(CacheKey, settings, TimeSpan.FromMinutes(5));
        return settings;
    }

    /// <inheritdoc/>
    public async Task<SiteSettings> UpdateSettingsAsync(SiteSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _db.SiteSettings.Update(settings);
        await _db.SaveChangesAsync();

        // Xóa cache để request tiếp theo lấy data mới
        InvalidateCache();

        _logger.LogInformation("Cập nhật SiteSettings");
        return settings;
    }

    /// <inheritdoc/>
    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
    }
}
