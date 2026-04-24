using AquaCMS.Models.Entities;

namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service tìm kiếm thống nhất — dùng cho /api/search/suggest và trang search.
/// Hỗ trợ tìm sản phẩm + bài viết bằng full-text (pg_trgm).
/// </summary>
public interface ISearchService
{
    /// <summary>Suggestion ngắn cho dropdown autocomplete (tối đa 8 kết quả)</summary>
    Task<SearchSuggestionResult> SuggestAsync(string query, int limit = 8);
}

public class SearchSuggestionResult
{
    public List<SearchItem> Products { get; set; } = new();
    public List<SearchItem> Posts { get; set; } = new();
    public int TotalProducts { get; set; }
    public int TotalPosts { get; set; }
}

public class SearchItem
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Subtitle { get; set; }
}
