using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;

namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service quản lý bài viết kiến thức (Posts) + danh mục kiến thức.
/// </summary>
public interface IKnowledgeService
{
    // ===== Public =====
    Task<PaginatedList<Post>> GetPublishedPostsAsync(string? categorySlug = null, int page = 1, int pageSize = 12);
    Task<Post?> GetBySlugAsync(string slug);
    Task<Post?> GetByShortIdAsync(long shortId);
    Task<List<Post>> GetLatestPostsAsync(int count = 6);
    Task IncrementViewCountAsync(Guid postId);

    // ===== Categories =====
    Task<List<KnowledgeCategory>> GetCategoriesAsync();
    Task<KnowledgeCategory?> GetCategoryBySlugAsync(string slug);

    // ===== Admin CRUD =====
    Task<PaginatedList<Post>> GetAdminPostsAsync(string? search = null, int page = 1, int pageSize = 20);
    Task<Post?> GetByIdAsync(Guid id);
    Task<Post> CreateAsync(Post post);
    Task<Post> UpdateAsync(Post post);
    Task DeleteAsync(Guid id);

    // ===== Category CRUD =====
    Task<KnowledgeCategory?> GetCategoryByIdAsync(Guid id);
    Task<KnowledgeCategory> CreateCategoryAsync(KnowledgeCategory cat);
    Task<KnowledgeCategory> UpdateCategoryAsync(KnowledgeCategory cat);
    Task DeleteCategoryAsync(Guid id);
}
