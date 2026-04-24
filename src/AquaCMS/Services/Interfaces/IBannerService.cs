using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;

namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service quản lý banner trang chủ.
/// </summary>
public interface IBannerService
{
    Task<List<Banner>> GetActiveBannersAsync();
    Task<PaginatedList<Banner>> GetAdminBannersAsync(int page = 1, int pageSize = 20);
    Task<Banner?> GetByIdAsync(Guid id);
    Task<Banner> CreateAsync(Banner banner);
    Task<Banner> UpdateAsync(Banner banner);
    Task DeleteAsync(Guid id);
}
