using System.Globalization;

namespace AquaCMS.Helpers;

/// <summary>
/// Helper hiển thị giá sản phẩm.
/// null/0 → "Liên hệ báo giá", có giá → format VNĐ.
/// </summary>
public static class PriceHelper
{
    private static readonly CultureInfo ViCulture = new("vi-VN");

    /// <summary>
    /// Format giá tiền theo chuẩn VNĐ.
    /// </summary>
    /// <param name="price">Giá — null nghĩa là "Liên hệ"</param>
    /// <param name="showPrice">Nếu false -> luôn hiện "Liên hệ"</param>
    /// <returns>Chuỗi hiển thị (ví dụ: "5.000.000 ₫" hoặc "Liên hệ báo giá")</returns>
    public static string FormatPrice(decimal? price, bool showPrice = true)
    {
        if (!showPrice || price is null or 0)
            return "Liên hệ";

        return price.Value.ToString("N0", ViCulture) + " ₫";
    }
}
