using System.ComponentModel.DataAnnotations;

namespace AquaCMS.Models.Entities;

/// <summary>
/// Lưu trữ dữ liệu thay đổi thường xuyên (High-write frequency).
/// </summary>
public class ProductStatistic
{
    [Key]
    public Guid ProductId { get; set; }

    /// <summary>Đếm lượt xem</summary>
    public int ViewCount { get; set; }

    // Navigation
    public virtual Product? Product { get; set; }
}
