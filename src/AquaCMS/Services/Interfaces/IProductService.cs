using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;

namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service quản lý sản phẩm — business logic layer.
/// Controller chỉ gọi service, không query DB trực tiếp.
/// </summary>
public interface IProductService
{
    /// <summary>Lấy danh sách sản phẩm cho trang public (không hiện hidden)</summary>
    Task<PaginatedList<Product>> GetPublicProductsAsync(
        string? search = null, string? categorySlug = null,
        int page = 1, int pageSize = 12);

    /// <summary>Lấy sản phẩm theo slug — dùng cho trang detail</summary>
    Task<Product?> GetBySlugAsync(string slug);

    /// <summary>Lấy sản phẩm theo short_id (URL SEO /slug-12345)</summary>
    Task<Product?> GetByShortIdAsync(long shortId);

    /// <summary>Tăng lượt xem sản phẩm +1 (fire-and-forget)</summary>
    Task IncrementViewCountAsync(Guid productId);

    /// <summary>Lấy sản phẩm nổi bật cho trang chủ</summary>
    Task<List<Product>> GetFeaturedProductsAsync(int count = 8);

    /// <summary>Lấy sản phẩm cùng danh mục (cross-sell)</summary>
    Task<List<Product>> GetRelatedProductsAsync(Guid productId, Guid? categoryId, int count = 4);

    // ===== Admin CRUD =====

    /// <summary>Lấy tất cả sản phẩm cho admin (kể cả hidden)</summary>
    Task<PaginatedList<Product>> GetAdminProductsAsync(
        string? search = null, string? status = null,
        int page = 1, int pageSize = 20);

    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Guid id);

    /// <summary>Thao tác hàng loạt: ẩn/hiện/xóa nhiều sản phẩm</summary>
    Task<int> BulkActionAsync(List<Guid> ids, string action);

    /// <summary>Lấy nhiều sản phẩm theo danh sách ID (cho bulk edit)</summary>
    Task<List<Product>> GetByIdsAsync(List<Guid> ids);

    /// <summary>Cập nhật giá và trạng thái hàng loạt</summary>
    Task<int> BulkUpdateAsync(List<Guid> ids, List<decimal?> prices, List<ProductStatus> statuses);
}
