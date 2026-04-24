namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service generate sitemap.xml động từ DB (sản phẩm + bài viết + đối tác + danh mục).
/// </summary>
public interface ISitemapService
{
    /// <summary>Sinh XML sitemap (cached 1 giờ trong memory)</summary>
    Task<string> GenerateSitemapAsync(string baseUrl);
}
