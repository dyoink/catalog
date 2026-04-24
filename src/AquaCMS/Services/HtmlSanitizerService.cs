using AquaCMS.Services.Interfaces;
using Ganss.Xss;

namespace AquaCMS.Services;

/// <summary>
/// Wrapper quanh thư viện HtmlSanitizer 9.0 (Ganss.Xss).
/// Whitelist: các thẻ HTML phổ biến cho rich text editor TinyMCE,
/// loại bỏ script/iframe/style/object/embed/onclick...
/// </summary>
public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;
    private readonly ILogger<HtmlSanitizerService> _logger;

    public HtmlSanitizerService(ILogger<HtmlSanitizerService> logger)
    {
        _logger = logger;

        _sanitizer = new HtmlSanitizer();

        // Cho phép YouTube/Vimeo embed
        _sanitizer.AllowedTags.Add("iframe");
        _sanitizer.AllowedAttributes.Add("allow");
        _sanitizer.AllowedAttributes.Add("allowfullscreen");
        _sanitizer.AllowedAttributes.Add("frameborder");

        // Cho phép data-* attribute (TinyMCE dùng cho lazy load, tracking)
        _sanitizer.AllowDataAttributes = true;

        // Whitelist scheme cho href/src
        _sanitizer.AllowedSchemes.Add("tel");
        _sanitizer.AllowedSchemes.Add("mailto");

        // Sự kiện: HtmlSanitizer log khi remove element/attribute (phục vụ debug XSS attempt)
        _sanitizer.RemovingTag += (sender, e) =>
            _logger.LogWarning("Sanitizer xóa thẻ độc hại: {Tag}", e.Tag.NodeName);
        _sanitizer.RemovingAttribute += (sender, e) =>
            _logger.LogWarning("Sanitizer xóa attribute độc hại: {Attr}", e.Attribute.Name);
    }

    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        try
        {
            return _sanitizer.Sanitize(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi sanitize HTML");
            return string.Empty;
        }
    }
}
