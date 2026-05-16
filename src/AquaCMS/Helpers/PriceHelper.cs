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
    /// <param name="price">Giá gốc</param>
    /// <param name="discountPrice">Giá sau giảm</param>
    /// <param name="showPrice">Nếu false -> luôn hiện "Liên hệ"</param>
    /// <returns>Chuỗi hiển thị (ví dụ: "5.000.000 ₫" hoặc "Liên hệ báo giá")</returns>
    public static string FormatPrice(decimal? price, decimal? discountPrice = null, bool showPrice = true)
    {
        if (!showPrice)
            return "Liên hệ";

        var finalPrice = discountPrice ?? price;

        if (finalPrice is null or 0)
            return "Liên hệ";

        return finalPrice.Value.ToString("N0", ViCulture) + " ₫";
    }

    /// <summary>
    /// Tính phần trăm giảm giá.
    /// </summary>
    public static int GetDiscountPercentage(decimal? originalPrice, decimal? discountPrice)
    {
        if (originalPrice is null or <= 0 || discountPrice is null or <= 0 || discountPrice >= originalPrice)
            return 0;

        var percentage = (originalPrice.Value - discountPrice.Value) / originalPrice.Value * 100;
        return (int)Math.Round(percentage);
    }
}
