using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Models.Common;

/// <summary>
/// Danh sách phân trang — dùng chung cho tất cả entity.
/// Hỗ trợ tính toán tổng số trang, trang hiện tại, có trang trước/sau không.
/// </summary>
/// <typeparam name="T">Kiểu entity</typeparam>
public class PaginatedList<T>
{
    /// <summary>Danh sách items trong trang hiện tại</summary>
    public List<T> Items { get; }

    /// <summary>Trang hiện tại (1-based)</summary>
    public int PageIndex { get; }

    /// <summary>Tổng số trang</summary>
    public int TotalPages { get; }

    /// <summary>Tổng số item (tất cả trang)</summary>
    public int TotalCount { get; }

    /// <summary>Số item mỗi trang</summary>
    public int PageSize { get; }

    public PaginatedList(List<T> items, int totalCount, int pageIndex, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>Có trang trước không — dùng cho UI phân trang</summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>Có trang sau không — dùng cho UI phân trang</summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// Tạo PaginatedList từ IQueryable — chỉ query đúng page cần thiết.
    /// </summary>
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageIndex, int pageSize)
    {
        // Đếm tổng số records (1 query COUNT)
        var totalCount = await source.CountAsync();

        // Lấy đúng page cần (1 query OFFSET/LIMIT)
        var items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<T>(items, totalCount, pageIndex, pageSize);
    }
}
