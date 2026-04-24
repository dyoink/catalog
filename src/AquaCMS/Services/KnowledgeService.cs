using AquaCMS.Data;
using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Services;

/// <summary>
/// Knowledge service — quản lý bài viết kiến thức.
/// </summary>
public class KnowledgeService : IKnowledgeService
{
    private readonly AppDbContext _db;

    public KnowledgeService(AppDbContext db) => _db = db;

    // ===== Public =====

    public async Task<PaginatedList<Post>> GetPublishedPostsAsync(
        string? categorySlug = null, int page = 1, int pageSize = 12)
    {
        var query = _db.Posts
            .Include(p => p.KnowledgeCategory)
            .Where(p => p.IsPublished)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            query = query.Where(p => p.KnowledgeCategory != null
                && p.KnowledgeCategory.Slug == categorySlug);
        }

        query = query.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt);

        return await PaginatedList<Post>.CreateAsync(query, page, pageSize);
    }

    public async Task<Post?> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        return await _db.Posts
            .Include(p => p.KnowledgeCategory)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);
    }

    public async Task<Post?> GetByShortIdAsync(long shortId)
    {
        if (shortId <= 0) return null;
        return await _db.Posts
            .Include(p => p.KnowledgeCategory)
            .FirstOrDefaultAsync(p => p.ShortId == shortId && p.IsPublished);
    }

    public async Task<List<Post>> GetLatestPostsAsync(int count = 6)
    {
        return await _db.Posts
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(Guid postId)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE posts SET view_count = view_count + 1 WHERE id = {postId}");
    }

    // ===== Categories =====

    public async Task<List<KnowledgeCategory>> GetCategoriesAsync()
    {
        return await _db.KnowledgeCategories
            .Include(c => c.Posts.Where(p => p.IsPublished))
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<KnowledgeCategory?> GetCategoryBySlugAsync(string slug)
    {
        return await _db.KnowledgeCategories
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    // ===== Admin CRUD =====

    public async Task<PaginatedList<Post>> GetAdminPostsAsync(
        string? search = null, int page = 1, int pageSize = 20)
    {
        var query = _db.Posts
            .Include(p => p.KnowledgeCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(term)
                || (p.Author != null && p.Author.ToLower().Contains(term)));
        }

        query = query.OrderByDescending(p => p.CreatedAt);
        return await PaginatedList<Post>.CreateAsync(query, page, pageSize);
    }

    public async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _db.Posts
            .Include(p => p.KnowledgeCategory)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post> CreateAsync(Post post)
    {
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();
        return post;
    }

    public async Task<Post> UpdateAsync(Post post)
    {
        _db.Posts.Update(post);
        await _db.SaveChangesAsync();
        return post;
    }

    public async Task DeleteAsync(Guid id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post != null)
        {
            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
        }
    }

    // ===== Category CRUD =====

    public async Task<KnowledgeCategory?> GetCategoryByIdAsync(Guid id) =>
        await _db.KnowledgeCategories.FindAsync(id);

    public async Task<KnowledgeCategory> CreateCategoryAsync(KnowledgeCategory cat)
    {
        _db.KnowledgeCategories.Add(cat);
        await _db.SaveChangesAsync();
        return cat;
    }

    public async Task<KnowledgeCategory> UpdateCategoryAsync(KnowledgeCategory cat)
    {
        _db.KnowledgeCategories.Update(cat);
        await _db.SaveChangesAsync();
        return cat;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var cat = await _db.KnowledgeCategories.FindAsync(id);
        if (cat != null)
        {
            _db.KnowledgeCategories.Remove(cat);
            await _db.SaveChangesAsync();
        }
    }
}
