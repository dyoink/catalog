namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Sanitize HTML content trước khi lưu DB — chống XSS.
/// Áp dụng cho: Post.Content, Partner.DetailedDescription, SiteSettings.AboutContent.
/// </summary>
public interface IHtmlSanitizerService
{
    /// <summary>Làm sạch HTML, loại bỏ script/iframe/onclick độc hại</summary>
    string Sanitize(string? html);
}
