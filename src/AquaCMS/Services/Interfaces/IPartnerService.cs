using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;

namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service quản lý đối tác (Partners) + danh mục đối tác.
/// </summary>
public interface IPartnerService
{
    // ===== Public =====
    Task<PaginatedList<Partner>> GetActivePartnersAsync(string? categorySlug = null, int page = 1, int pageSize = 12);
    Task<Partner?> GetBySlugAsync(string slug);
    Task<Partner?> GetByShortIdAsync(long shortId);

    // ===== Categories =====
    Task<List<PartnerCategory>> GetCategoriesAsync();

    // ===== Admin CRUD =====
    Task<PaginatedList<Partner>> GetAdminPartnersAsync(string? search = null, int page = 1, int pageSize = 20);
    Task<Partner?> GetByIdAsync(Guid id);
    Task<Partner> CreateAsync(Partner partner);
    Task<Partner> UpdateAsync(Partner partner);
    Task DeleteAsync(Guid id);

    // Category CRUD
    Task<PartnerCategory?> GetCategoryByIdAsync(Guid id);
    Task<PartnerCategory> CreateCategoryAsync(PartnerCategory cat);
    Task<PartnerCategory> UpdateCategoryAsync(PartnerCategory cat);
    Task DeleteCategoryAsync(Guid id);
}
