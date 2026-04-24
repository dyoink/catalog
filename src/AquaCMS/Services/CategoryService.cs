using AquaCMS.Data;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Services;

/// <summary>
/// Implementation CategoryService — quản lý danh mục sản phẩm.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<List<Category>> GetAllWithCountAsync()
    {
        // Lấy tất cả danh mục kèm số lượng sản phẩm visible
        return await _db.Categories
            .Include(c => c.Products.Where(p => p.Status != ProductStatus.Hidden))
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _db.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    /// <inheritdoc/>
    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _db.Categories.FindAsync(id);
    }

    /// <inheritdoc/>
    public async Task<Category> CreateAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    /// <inheritdoc/>
    public async Task<Category> UpdateAsync(Category category)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
        return category;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category != null)
        {
            // Products thuộc category này sẽ có CategoryId = NULL (ON DELETE SET NULL)
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }
    }
}
