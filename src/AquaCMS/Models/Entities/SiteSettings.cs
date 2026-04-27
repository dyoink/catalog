namespace AquaCMS.Models.Entities;

/// <summary>
/// Cấu hình hệ thống — singleton, chỉ có 1 row duy nhất.
/// Chứa toàn bộ thông tin doanh nghiệp, giao diện, liên hệ, CMS toggles.
/// </summary>
public class SiteSettings
{
    public Guid Id { get; set; }

    // ===== Thông tin doanh nghiệp =====
    public string CompanyName { get; set; } = "AquaCMS";
    public string? Logo { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // ===== Mạng xã hội — URL + Toggle bật/tắt floating buttons =====
    public string? Facebook { get; set; }
    public bool ShowFacebook { get; set; } = true;
    public string? Zalo { get; set; }
    public bool ShowZalo { get; set; } = true;
    public string? Youtube { get; set; }
    public bool ShowYoutube { get; set; }
    public string? Tiktok { get; set; }
    public bool ShowTiktok { get; set; }
    public string? Telegram { get; set; }
    public bool ShowTelegram { get; set; }
    public bool ShowHotline { get; set; } = true;

    // ===== Thanh toán =====
    public string? BankName { get; set; }
    public string? BankNumber { get; set; }
    public string? BankOwner { get; set; }

    // ===== Giao diện =====
    public string BackgroundColor { get; set; } = "#F9F9F9";
    public string PrimaryColor { get; set; } = "#55B3D9";
    public string? FooterText { get; set; }
    public bool ShowFooter { get; set; } = true;
    public string? HeroBackgroundImage { get; set; }

    // ===== CMS — Homepage module toggles =====
    public bool ShowBanners { get; set; } = true;
    public bool ShowCategories { get; set; } = true;
    public bool ShowFeaturedProducts { get; set; } = true;
    public bool ShowLatestPosts { get; set; } = true;
    public bool ShowPartners { get; set; } = true;
    public int FeaturedProductsCount { get; set; } = 8;
    public int LatestPostsCount { get; set; } = 6;

    // ===== CMS — Navbar toggles =====
    public bool ShowNavProducts { get; set; } = true;
    public bool ShowNavKnowledge { get; set; } = true;
    public bool ShowNavPartners { get; set; } = true;
    public bool ShowNavCart { get; set; } = true;

    // ===== CMS — Hero / Intro section (chỉnh sửa từ admin) =====
    public string? HeroTitle { get; set; }
    public string? HeroSubtitle { get; set; }
    public string? HeroDescription { get; set; }
    public string? HeroButtonText { get; set; }
    public string? HeroButtonUrl { get; set; }

    // ===== CMS — Giới thiệu (about/intro) =====
    public string? AboutTitle { get; set; }
    public string? AboutContent { get; set; }
    public string? AboutImage { get; set; }

    // ===== SEO mặc định =====
    public string? DefaultMetaTitle { get; set; }
    public string? DefaultMetaDescription { get; set; }
    public string? DefaultOgImage { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? FacebookPixelId { get; set; }

    // ===== Footer extras =====
    public string? FooterAboutText { get; set; }
    public string? CopyrightText { get; set; }

    // ===== Email / SMTP (cấu hình từ admin) =====
    public bool EmailEnabled { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public bool SmtpUseSsl { get; set; } = true;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName { get; set; }
    /// <summary>Email nhận thông báo (tin nhắn mới, đơn hàng...).</summary>
    public string? NotificationEmail { get; set; }

    // ===== Chat Auto Reply =====
    public string? ChatAutoReplyMessage { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}