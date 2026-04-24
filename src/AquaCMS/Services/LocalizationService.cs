using System.Collections.Concurrent;

namespace AquaCMS.Services;

/// <summary>
/// Service dịch UI strings dạng dictionary đơn giản — không cần .resx.
/// Cookie `culture` ("vi" hoặc "en") quyết định ngôn ngữ.
/// Nếu key không có bản dịch → trả về key gốc (fail-safe).
/// </summary>
public interface ILocalizationService
{
    /// <summary>Văn hoá hiện tại ("vi" hoặc "en")</summary>
    string CurrentCulture { get; }

    /// <summary>Dịch 1 key sang ngôn ngữ hiện tại</summary>
    string T(string key);
}

public class LocalizationService : ILocalizationService
{
    public const string CookieName = "AquaCMS.Lang";
    public const string DefaultCulture = "vi";
    public static readonly string[] SupportedCultures = { "vi", "en" };

    private readonly IHttpContextAccessor _http;

    public LocalizationService(IHttpContextAccessor http) => _http = http;

    public string CurrentCulture
    {
        get
        {
            var cookie = _http.HttpContext?.Request.Cookies[CookieName];
            if (!string.IsNullOrEmpty(cookie) && SupportedCultures.Contains(cookie))
                return cookie;
            return DefaultCulture;
        }
    }

    public string T(string key)
    {
        var dict = CurrentCulture == "en" ? _en : _vi;
        return dict.TryGetValue(key, out var v) ? v : key;
    }

    // ============================================================
    // Dictionary từ vựng — thêm key mới ở cả 2 ngôn ngữ
    // Đặt key dạng "scope.subkey" cho dễ tìm.
    // ============================================================

    private static readonly ConcurrentDictionary<string, string> _vi = new(new Dictionary<string, string>
    {
        // Navigation
        ["nav.home"] = "Trang chủ",
        ["nav.products"] = "Sản phẩm",
        ["nav.knowledge"] = "Kiến thức",
        ["nav.partners"] = "Đối tác",
        ["nav.cart"] = "Giỏ hàng",
        ["nav.search.placeholder"] = "Tìm sản phẩm...",
        ["nav.search.hint"] = "Gõ để tìm kiếm...",
        ["nav.menu"] = "Menu",

        // Common
        ["common.viewAll"] = "Xem tất cả",
        ["common.readMore"] = "Đọc tiếp",
        ["common.contact"] = "Liên hệ",
        ["common.callNow"] = "Gọi ngay",
        ["common.viewDetail"] = "Xem chi tiết",
        ["common.addToCart"] = "Thêm vào giỏ",
        ["common.buyNow"] = "Mua ngay",
        ["common.contactForPrice"] = "Liên hệ báo giá",
        ["common.related"] = "Sản phẩm liên quan",
        ["common.search"] = "Tìm kiếm",
        ["common.filter"] = "Lọc",
        ["common.all"] = "Tất cả",
        ["common.noData"] = "Chưa có dữ liệu",
        ["common.loading"] = "Đang tải...",
        ["common.back"] = "Quay lại",

        // Footer
        ["footer.quickLinks"] = "Liên kết nhanh",
        ["footer.contact"] = "Liên hệ",
        ["footer.followUs"] = "Theo dõi chúng tôi",
        ["footer.address"] = "Địa chỉ",
        ["footer.phone"] = "Điện thoại",
        ["footer.email"] = "Email",

        // Pages
        ["page.home"] = "Trang chủ",
        ["page.products"] = "Sản phẩm",
        ["page.knowledge"] = "Bài viết kiến thức",
        ["page.partners"] = "Đối tác",
        ["page.cart"] = "Giỏ hàng của bạn",
        ["page.featuredProducts"] = "Sản phẩm nổi bật",
        ["page.latestPosts"] = "Bài viết mới",
        ["page.ourPartners"] = "Đối tác của chúng tôi",
        ["page.aboutUs"] = "Về chúng tôi",

        // Lang switch
        ["lang.switch"] = "English",
    });

    private static readonly ConcurrentDictionary<string, string> _en = new(new Dictionary<string, string>
    {
        ["nav.home"] = "Home",
        ["nav.products"] = "Products",
        ["nav.knowledge"] = "Knowledge",
        ["nav.partners"] = "Partners",
        ["nav.cart"] = "Cart",
        ["nav.search.placeholder"] = "Search products...",
        ["nav.search.hint"] = "Start typing to search...",
        ["nav.menu"] = "Menu",

        ["common.viewAll"] = "View all",
        ["common.readMore"] = "Read more",
        ["common.contact"] = "Contact",
        ["common.callNow"] = "Call now",
        ["common.viewDetail"] = "View details",
        ["common.addToCart"] = "Add to cart",
        ["common.buyNow"] = "Buy now",
        ["common.contactForPrice"] = "Contact for price",
        ["common.related"] = "Related products",
        ["common.search"] = "Search",
        ["common.filter"] = "Filter",
        ["common.all"] = "All",
        ["common.noData"] = "No data yet",
        ["common.loading"] = "Loading...",
        ["common.back"] = "Back",

        ["footer.quickLinks"] = "Quick links",
        ["footer.contact"] = "Contact",
        ["footer.followUs"] = "Follow us",
        ["footer.address"] = "Address",
        ["footer.phone"] = "Phone",
        ["footer.email"] = "Email",

        ["page.home"] = "Home",
        ["page.products"] = "Products",
        ["page.knowledge"] = "Knowledge base",
        ["page.partners"] = "Partners",
        ["page.cart"] = "Your cart",
        ["page.featuredProducts"] = "Featured products",
        ["page.latestPosts"] = "Latest posts",
        ["page.ourPartners"] = "Our partners",
        ["page.aboutUs"] = "About us",

        ["lang.switch"] = "Tiếng Việt",
    });
}
