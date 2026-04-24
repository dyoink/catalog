using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AquaCMS.Helpers;

/// <summary>
/// Helper tạo URL slug SEO-friendly từ chuỗi tiếng Việt.
/// Ví dụ: "Máy Cho Tôm Ăn 360" → "may-cho-tom-an-360"
/// </summary>
public static partial class SlugHelper
{
    // Bảng chuyển đổi ký tự tiếng Việt → không dấu
    private static readonly Dictionary<char, string> VietnameseMap = new()
    {
        ['à'] = "a", ['á'] = "a", ['ả'] = "a", ['ã'] = "a", ['ạ'] = "a",
        ['ă'] = "a", ['ằ'] = "a", ['ắ'] = "a", ['ẳ'] = "a", ['ẵ'] = "a", ['ặ'] = "a",
        ['â'] = "a", ['ầ'] = "a", ['ấ'] = "a", ['ẩ'] = "a", ['ẫ'] = "a", ['ậ'] = "a",
        ['đ'] = "d",
        ['è'] = "e", ['é'] = "e", ['ẻ'] = "e", ['ẽ'] = "e", ['ẹ'] = "e",
        ['ê'] = "e", ['ề'] = "e", ['ế'] = "e", ['ể'] = "e", ['ễ'] = "e", ['ệ'] = "e",
        ['ì'] = "i", ['í'] = "i", ['ỉ'] = "i", ['ĩ'] = "i", ['ị'] = "i",
        ['ò'] = "o", ['ó'] = "o", ['ỏ'] = "o", ['õ'] = "o", ['ọ'] = "o",
        ['ô'] = "o", ['ồ'] = "o", ['ố'] = "o", ['ổ'] = "o", ['ỗ'] = "o", ['ộ'] = "o",
        ['ơ'] = "o", ['ờ'] = "o", ['ớ'] = "o", ['ở'] = "o", ['ỡ'] = "o", ['ợ'] = "o",
        ['ù'] = "u", ['ú'] = "u", ['ủ'] = "u", ['ũ'] = "u", ['ụ'] = "u",
        ['ư'] = "u", ['ừ'] = "u", ['ứ'] = "u", ['ử'] = "u", ['ữ'] = "u", ['ự'] = "u",
        ['ỳ'] = "y", ['ý'] = "y", ['ỷ'] = "y", ['ỹ'] = "y", ['ỵ'] = "y",
    };

    /// <summary>
    /// Tạo slug từ chuỗi bất kỳ (hỗ trợ tiếng Việt).
    /// Bỏ dấu → lowercase → thay space bằng dash → xóa ký tự đặc biệt.
    /// </summary>
    /// <param name="input">Chuỗi gốc (ví dụ: "Máy Cho Tôm Ăn 360 - X50")</param>
    /// <returns>Slug (ví dụ: "may-cho-tom-an-360-x50")</returns>
    public static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Bước 1: Lowercase
        var slug = input.ToLowerInvariant();

        // Bước 2: Thay thế ký tự tiếng Việt
        var sb = new StringBuilder(slug.Length);
        foreach (var c in slug)
        {
            if (VietnameseMap.TryGetValue(c, out var replacement))
                sb.Append(replacement);
            else
                sb.Append(c);
        }
        slug = sb.ToString();

        // Bước 3: Normalize unicode còn sót (ví dụ: ñ, ü)
        slug = slug.Normalize(NormalizationForm.FormD);
        slug = NonAsciiLetterRegex().Replace(slug, "");

        // Bước 4: Thay mọi ký tự không phải alphanumeric bằng dash
        slug = NonAlphanumericRegex().Replace(slug, "-");

        // Bước 5: Gộp nhiều dash liên tiếp thành 1
        slug = MultipleDashRegex().Replace(slug, "-");

        // Bước 6: Xóa dash ở đầu và cuối
        slug = slug.Trim('-');

        return slug;
    }

    // Regex compiled — performance tốt hơn khi gọi nhiều lần
    [GeneratedRegex(@"\p{Mn}", RegexOptions.Compiled)]
    private static partial Regex NonAsciiLetterRegex();

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-{2,}", RegexOptions.Compiled)]
    private static partial Regex MultipleDashRegex();
}
