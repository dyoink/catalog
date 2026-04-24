using AquaCMS.Data;
using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Services;

/// <summary>
/// Partner service — quản lý đối tác.
/// </summary>
public class PartnerService : IPartnerService
{
    private readonly AppDbContext _db;

    public PartnerService(AppDbContext db) => _db = db;

    public async Task<PaginatedList<Partner>> GetActivePartnersAsync(
        string? categorySlug = null, int page = 1, int pageSize = 12)
    {
        var query = _db.Partners
            .Include(p => p.PartnerCategory)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            query = query.Where(p => p.PartnerCategory != null
                && p.PartnerCategory.Slug == categorySlug);
        }

        query = query.OrderBy(p => p.SortOrder).ThenBy(p => p.Name);
        return await PaginatedList<Partner>.CreateAsync(query, page, pageSize);
    }

    public async Task<Partner?> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        return await _db.Partners
            .Include(p => p.PartnerCategory)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
    }

    public async Task<Partner?> GetByShortIdAsync(long shortId)
    {
        if (shortId <= 0) return null;
        return await _db.Partners
            .Include(p => p.PartnerCategory)
            .FirstOrDefaultAsync(p => p.ShortId == shortId && p.IsActive);
    }

    public async Task<List<PartnerCategory>> GetCategoriesAsync()
    {
        return await _db.PartnerCategories
            .Include(c => c.Partners.Where(p => p.IsActive))
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    // ===== Admin CRUD =====

    public async Task<PaginatedList<Partner>> GetAdminPartnersAsync(
        string? search = null, int page = 1, int pageSize = 20)
    {
        var query = _db.Partners
            .Include(p => p.PartnerCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        query = query.OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt);
        return await PaginatedList<Partner>.CreateAsync(query, page, pageSize);
    }

    public async Task<Partner?> GetByIdAsync(Guid id)
    {
        return await _db.Partners
            .Include(p => p.PartnerCategory)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Partner> CreateAsync(Partner partner)
    {
        _db.Partners.Add(partner);
        await _db.SaveChangesAsync();
        return partner;
    }

    public async Task<Partner> UpdateAsync(Partner partner)
    {
        _db.Partners.Update(partner);
        await _db.SaveChangesAsync();
        return partner;
    }

    public async Task DeleteAsync(Guid id)
    {
        var p = await _db.Partners.FindAsync(id);
        if (p != null)
        {
            _db.Partners.Remove(p);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<PartnerCategory?> GetCategoryByIdAsync(Guid id) =>
        await _db.PartnerCategories.FindAsync(id);

    public async Task<PartnerCategory> CreateCategoryAsync(PartnerCategory cat)
    {
        _db.PartnerCategories.Add(cat);
        await _db.SaveChangesAsync();
        return cat;
    }

    public async Task<PartnerCategory> UpdateCategoryAsync(PartnerCategory cat)
    {
        _db.PartnerCategories.Update(cat);
        await _db.SaveChangesAsync();
        return cat;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var cat = await _db.PartnerCategories.FindAsync(id);
        if (cat != null)
        {
            _db.PartnerCategories.Remove(cat);
            await _db.SaveChangesAsync();
        }
    }
}
