namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service upload file (ảnh sản phẩm, banner, avatar) lên local storage wwwroot/uploads/.
/// Trả về URL relative để lưu DB, validate file type và size.
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Upload 1 ảnh, trả về URL public (vd: /uploads/products/abc.jpg).
    /// </summary>
    /// <param name="file">File từ form upload</param>
    /// <param name="folder">Thư mục con dưới wwwroot/uploads (vd: "products", "banners", "avatars")</param>
    /// <returns>URL relative đã lưu (null nếu lỗi)</returns>
    Task<string?> UploadImageAsync(IFormFile? file, string folder);

    /// <summary>
    /// Xóa file đã upload (theo URL).
    /// </summary>
    bool DeleteFile(string? url);
}
