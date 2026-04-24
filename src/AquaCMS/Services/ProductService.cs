using AquaCMS.Data;
using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Services;

/// <summary>
/// Implementation ProductService — toàn bộ business logic sản phẩm.
/// Sử dụng AppDbContext trực tiếp (không qua generic repository vì query phức tạp).
/// </summary>
public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductService> _logger;

    public ProductService(AppDbContext db, ILogger<ProductService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedList<Product>> GetPublicProductsAsync(
        string? search = null, string? categorySlug = null,
        int page = 1, int pageSize = 12)
    {
        // Bắt đầu từ query base — chỉ lấy sản phẩm không bị ẩn
        var query = _db.Products
            .Include(p => p.Category)
            .Where(p => p.Status != ProductStatus.Hidden)
            .AsQueryable();

        // Lọc theo danh mục (nếu có)
        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            query = query.Where(p =>
                p.Category != null && p.Category.Slug == categorySlug);
        }

        // Tìm kiếm theo tên hoặc SKU (ILIKE — case-insensitive)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{searchLower}%") ||
                (p.Sku != null && EF.Functions.ILike(p.Sku, $"%{searchLower}%")));
        }

        // Sắp xếp: sản phẩm mới nhất trước
        query = query.OrderByDescending(p => p.CreatedAt);

        return await PaginatedList<Product>.CreateAsync(query, page, pageSize);
    }

    /// <inheritdoc/>
    public async Task<Product?> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        return await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    /// <inheritdoc/>
    public async Task<Product?> GetByShortIdAsync(long shortId)
    {
        if (shortId <= 0) return null;
        return await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ShortId == shortId);
    }

    /// <inheritdoc/>
    public async Task IncrementViewCountAsync(Guid productId)
    {
        // Dùng raw SQL để tăng atomic, tránh race condition
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE products SET view_count = view_count + 1 WHERE id = {productId}");
    }

    /// <inheritdoc/>
    public async Task<List<Product>> GetFeaturedProductsAsync(int count = 8)
    {
        return await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Status != ProductStatus.Hidden && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<Product>> GetRelatedProductsAsync(
        Guid productId, Guid? categoryId, int count = 4)
    {
        return await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Id != productId
                     && p.Status != ProductStatus.Hidden
                     && p.CategoryId == categoryId)
            .OrderByDescending(p => p.ViewCount)
            .Take(count)
            .ToListAsync();
    }

    // ===== Admin CRUD =====

    /// <inheritdoc/>
    public async Task<PaginatedList<Product>> GetAdminProductsAsync(
        string? search = null, string? status = null,
        int page = 1, int pageSize = 20)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .AsQueryable();

        // Admin có thể lọc theo status cụ thể
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProductStatus>(status, true, out var statusEnum))
        {
            query = query.Where(p => p.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{searchLower}%") ||
                (p.Sku != null && EF.Functions.ILike(p.Sku, $"%{searchLower}%")));
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        return await PaginatedList<Product>.CreateAsync(query, page, pageSize);
    }

    /// <inheritdoc/>
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Product> CreateAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Tạo sản phẩm mới: {Name} (ID: {Id})", product.Name, product.Id);
        return product;
    }

    /// <inheritdoc/>
    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Cập nhật sản phẩm: {Name} (ID: {Id})", product.Name, product.Id);
        return product;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Xóa sản phẩm: {Name} (ID: {Id})", product.Name, product.Id);
        }
    }

    /// <inheritdoc/>
    public async Task<int> BulkActionAsync(List<Guid> ids, string action)
    {
        var products = await _db.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        switch (action.ToLower())
        {
            case "hide":
                products.ForEach(p => p.Status = ProductStatus.Hidden);
                break;
            case "show":
                products.ForEach(p => p.Status = ProductStatus.Available);
                break;
            case "delete":
                _db.Products.RemoveRange(products);
                break;
            default:
                return 0;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Bulk action '{Action}' trên {Count} sản phẩm", action, products.Count);
        return products.Count;
    }

    /// <inheritdoc/>
    public async Task<List<Product>> GetByIdsAsync(List<Guid> ids)
    {
        return await _db.Products
            .Include(p => p.Category)
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<int> BulkUpdateAsync(List<Guid> ids, List<decimal?> prices, List<ProductStatus> statuses)
    {
        var products = await _db.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        var dict = products.ToDictionary(p => p.Id);
        var count = 0;

        for (int i = 0; i < ids.Count; i++)
        {
            if (dict.TryGetValue(ids[i], out var product))
            {
                product.Price = prices[i];
                product.Status = statuses[i];
                product.UpdatedAt = DateTime.UtcNow;
                count++;
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Bulk update {Count} sản phẩm (giá + trạng thái)", count);
        return count;
    }
}
