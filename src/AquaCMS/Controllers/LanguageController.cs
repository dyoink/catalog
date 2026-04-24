using AquaCMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Controllers;

/// <summary>Đổi ngôn ngữ — set cookie và redirect về trang trước.</summary>
[Route("dat-ngon-ngu")]
public class LanguageController : Controller
{
    /// <summary>GET /dat-ngon-ngu/{culture}?returnUrl=...</summary>
    [HttpGet("{culture}")]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        if (!LocalizationService.SupportedCultures.Contains(culture))
            culture = LocalizationService.DefaultCulture;

        Response.Cookies.Append(
            LocalizationService.CookieName,
            culture,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false,         // JS có thể đọc nếu cần
                SameSite = SameSiteMode.Lax,
                IsEssential = true         // Cần thiết → không bị GDPR consent block
            });

        // Redirect an toàn — chỉ chấp nhận local URL
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return Redirect("/");
    }
}
