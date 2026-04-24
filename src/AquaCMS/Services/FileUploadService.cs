using AquaCMS.Services.Interfaces;

namespace AquaCMS.Services;

/// <summary>
/// Implementation upload file lên local wwwroot/uploads/.
/// - Validate: chỉ accept image types (jpg, png, webp, gif), tối đa 5MB
/// - Tên file: GUID để tránh trùng + giữ extension gốc
/// - Bảo mật: kiểm tra magic bytes (signature), không tin extension client
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;
    private readonly IConfiguration _config;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger, IConfiguration config)
    {
        _env = env;
        _logger = logger;
        _config = config;
    }

    public async Task<string?> UploadImageAsync(IFormFile? file, string folder)
    {
        // Null safety
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("UploadImageAsync: file null hoặc rỗng");
            return null;
        }

        try
        {
            // Validate size
            var maxMb = _config.GetValue<int?>("Site:MaxUploadMb") ?? 5;
            var maxBytes = maxMb * 1024L * 1024L;
            if (file.Length > maxBytes)
            {
                _logger.LogWarning("File quá lớn: {Size} bytes (max {Max}MB)", file.Length, maxMb);
                throw new InvalidOperationException($"Ảnh tối đa {maxMb}MB");
            }

            // Validate extension
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
            if (!AllowedExtensions.Contains(ext))
            {
                _logger.LogWarning("Extension không hợp lệ: {Ext}", ext);
                throw new InvalidOperationException("Chỉ chấp nhận ảnh: jpg, png, webp, gif");
            }

            // Validate MIME type
            if (!AllowedMimeTypes.Contains(file.ContentType))
            {
                _logger.LogWarning("MIME type không hợp lệ: {Mime}", file.ContentType);
                throw new InvalidOperationException("File không phải là ảnh hợp lệ");
            }

            // Validate magic bytes (signature) — chống upload file giả mạo extension
            using (var checkStream = file.OpenReadStream())
            {
                if (!await IsValidImageSignatureAsync(checkStream, ext))
                {
                    _logger.LogWarning("Magic bytes không khớp với extension {Ext}", ext);
                    throw new InvalidOperationException("Nội dung file không phải ảnh hợp lệ");
                }
            }

            // Sanitize folder name (chỉ chữ-số-dash)
            var safeFolder = string.Concat(folder.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
            if (string.IsNullOrEmpty(safeFolder)) safeFolder = "misc";

            // Tạo thư mục nếu chưa có
            var webroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadDir = Path.Combine(webroot, "uploads", safeFolder);
            Directory.CreateDirectory(uploadDir);

            // Tên file ngẫu nhiên
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);

            // Save file
            using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            var publicUrl = $"/uploads/{safeFolder}/{fileName}";
            _logger.LogInformation("Upload thành công: {Url} ({Size} bytes)", publicUrl, file.Length);
            return publicUrl;
        }
        catch (InvalidOperationException)
        {
            // Đã log ở trên, ném lại để controller xử lý hiển thị
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi upload {FileName}", file.FileName);
            throw new InvalidOperationException("Lỗi hệ thống khi upload ảnh", ex);
        }
    }

    public bool DeleteFile(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/uploads/"))
            return false;

        try
        {
            var webroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            // Loại bỏ leading slash, normalize path
            var relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webroot, relativePath);

            // Bảo mật: kiểm tra path không thoát ra ngoài uploads
            var uploadsRoot = Path.GetFullPath(Path.Combine(webroot, "uploads"));
            var fullResolved = Path.GetFullPath(fullPath);
            if (!fullResolved.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("DeleteFile từ chối path không an toàn: {Url}", url);
                return false;
            }

            if (File.Exists(fullResolved))
            {
                File.Delete(fullResolved);
                _logger.LogInformation("Đã xóa file: {Url}", url);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xóa file {Url}", url);
            return false;
        }
    }

    /// <summary>Kiểm tra magic bytes (4-8 byte đầu) để xác định loại ảnh thực sự.</summary>
    private static async Task<bool> IsValidImageSignatureAsync(Stream stream, string ext)
    {
        var buffer = new byte[12];
        var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
        if (read < 4) return false;

        return ext switch
        {
            // JPEG: FF D8 FF
            ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
            // GIF: 47 49 46 38 (GIF8)
            ".gif" => buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38,
            // WEBP: "RIFF" ???? "WEBP"
            ".webp" => read >= 12 && buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46
                       && buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50,
            _ => false
        };
    }
}
