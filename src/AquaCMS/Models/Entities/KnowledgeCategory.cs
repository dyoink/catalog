namespace AquaCMS.Models.Entities;

/// <summary>
/// Danh mục bài viết kiến thức (ví dụ: "Tin tức", "Hướng dẫn", "Kỹ thuật").
/// </summary>
public class KnowledgeCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation =====
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
