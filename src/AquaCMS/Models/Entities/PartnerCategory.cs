namespace AquaCMS.Models.Entities;

/// <summary>
/// Danh mục đối tác (ví dụ: "Doanh nghiệp", "Hộ nuôi").
/// </summary>
public class PartnerCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation =====
    public ICollection<Partner> Partners { get; set; } = new List<Partner>();
}
