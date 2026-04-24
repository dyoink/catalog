using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AquaCMS.Controllers;

/// <summary>
/// JSON API tìm kiếm gợi ý — dùng cho navbar autocomplete (HTMX).
/// Rate limited.
/// </summary>
[EnableRateLimiting("api")]
public class SearchApiController : Controller
{
    private readonly ISearchService _search;
    private readonly ILogger<SearchApiController> _logger;

    public SearchApiController(ISearchService search, ILogger<SearchApiController> logger)
    {
        _search = search;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/search/suggest?q=...
    /// Trả về HTML partial cho HTMX (không phải JSON, để bind trực tiếp vào DOM).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Suggest(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return PartialView("_SearchSuggest", new SearchSuggestionResult());

        var result = await _search.SuggestAsync(q.Trim(), 6);
        ViewBag.Query = q;
        return PartialView("_SearchSuggest", result);
    }
}
