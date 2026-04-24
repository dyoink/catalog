using AquaCMS.Models.Entities;

namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service danh mục sản phẩm.
/// </summary>
public interface ICategoryService
{
    /// <summary>Lấy tất cả danh mục, kèm số lượng sản phẩm (visible)</summary>
    Task<List<Category>> GetAllWithCountAsync();

    /// <summary>Lấy danh mục theo slug</summary>
    Task<Category?> GetBySlugAsync(string slug);

    Task<Category?> GetByIdAsync(Guid id);
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task DeleteAsync(Guid id);
}
