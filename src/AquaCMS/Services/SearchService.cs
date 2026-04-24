using AquaCMS.Data;
using AquaCMS.Helpers;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Services;

/// <summary>
/// Implementation tìm kiếm — dùng EF Core + ILIKE (PostgreSQL).
/// Có thể nâng cấp dùng pg_trgm GIN index để nhanh hơn.
/// </summary>
public class SearchService : ISearchService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SearchService> _logger;

    public SearchService(AppDbContext db, ILogger<SearchService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SearchSuggestionResult> SuggestAsync(string query, int limit = 8)
    {
        var result = new SearchSuggestionResult();

        // Null safety
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return result;

        try
        {
            var pattern = $"%{query.Trim()}%";

            // Search products
            var productsQ = _db.Products
                .AsNoTracking()
                .Where(p => p.Status != ProductStatus.Hidden &&
                           (EF.Functions.ILike(p.Name, pattern) ||
                            (p.Description != null && EF.Functions.ILike(p.Description, pattern))));

            result.TotalProducts = await productsQ.CountAsync();
            var products = await productsQ
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.ViewCount)
                .Take(limit)
                .Select(p => new SearchItem
                {
                    Title = p.Name,
                    Url = $"/san-pham/{p.Slug}-{p.ShortId}",
                    Image = p.Image,
                    Subtitle = p.Price.HasValue ? PriceHelper.FormatPrice(p.Price) : "Liên hệ"
                })
                .ToListAsync();
            result.Products = products;

            // Search posts
            var postsQ = _db.Posts
                .AsNoTracking()
                .Where(p => p.IsPublished &&
                           (EF.Functions.ILike(p.Title, pattern) ||
                            (p.Excerpt != null && EF.Functions.ILike(p.Excerpt, pattern))));

            result.TotalPosts = await postsQ.CountAsync();
            var posts = await postsQ
                .OrderByDescending(p => p.PublishedAt)
                .Take(limit)
                .Select(p => new SearchItem
                {
                    Title = p.Title,
                    Url = $"/kien-thuc/{p.Slug}-{p.ShortId}",
                    Image = p.Image,
                    Subtitle = p.Excerpt
                })
                .ToListAsync();
            result.Posts = posts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi search suggestion với query='{Query}'", query);
        }

        return result;
    }
}
