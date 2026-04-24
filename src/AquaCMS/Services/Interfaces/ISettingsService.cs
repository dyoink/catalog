using AquaCMS.Models.Entities;

namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service cấu hình hệ thống — singleton settings.
/// Cache trong memory để tránh query DB mỗi request.
/// </summary>
public interface ISettingsService
{
    /// <summary>Lấy settings hiện tại (có cache)</summary>
    Task<SiteSettings> GetSettingsAsync();

    /// <summary>Cập nhật settings — xóa cache sau khi lưu</summary>
    Task<SiteSettings> UpdateSettingsAsync(SiteSettings settings);

    /// <summary>Xóa cache — gọi khi admin thay đổi settings</summary>
    void InvalidateCache();
}
