namespace AquaCMS.Models.Entities;

/// <summary>
/// Danh mục sản phẩm (flat — không có cấu trúc cây).
/// Ví dụ: "Máy cho ăn", "Máy sục khí", "Khung ao", "Phụ kiện"
/// </summary>
public class Category
{
    public Guid Id { get; set; }

    /// <summary>Tên danh mục</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Slug cho URL SEO-friendly (ví dụ: "may-cho-an")</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>URL ảnh đại diện danh mục</summary>
    public string? Image { get; set; }

    /// <summary>Thứ tự sắp xếp — số nhỏ hiện trước</summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation Properties =====

    /// <summary>Danh sách sản phẩm thuộc danh mục này</summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
