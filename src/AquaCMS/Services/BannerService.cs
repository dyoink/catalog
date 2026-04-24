using AquaCMS.Data;
using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Services;

public class BannerService : IBannerService
{
    private readonly AppDbContext _db;

    public BannerService(AppDbContext db) => _db = db;

    public async Task<List<Banner>> GetActiveBannersAsync()
    {
        return await _db.Banners
            .Where(b => b.IsActive)
            .OrderBy(b => b.SortOrder)
            .ToListAsync();
    }

    public async Task<PaginatedList<Banner>> GetAdminBannersAsync(int page = 1, int pageSize = 20)
    {
        var query = _db.Banners.OrderBy(b => b.SortOrder);
        return await PaginatedList<Banner>.CreateAsync(query, page, pageSize);
    }

    public async Task<Banner?> GetByIdAsync(Guid id)
        => await _db.Banners.FindAsync(id);

    public async Task<Banner> CreateAsync(Banner banner)
    {
        _db.Banners.Add(banner);
        await _db.SaveChangesAsync();
        return banner;
    }

    public async Task<Banner> UpdateAsync(Banner banner)
    {
        _db.Banners.Update(banner);
        await _db.SaveChangesAsync();
        return banner;
    }

    public async Task DeleteAsync(Guid id)
    {
        var b = await _db.Banners.FindAsync(id);
        if (b != null)
        {
            _db.Banners.Remove(b);
            await _db.SaveChangesAsync();
        }
    }
}
