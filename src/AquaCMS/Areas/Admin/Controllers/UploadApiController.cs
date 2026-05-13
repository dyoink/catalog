using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "EditorUp")]
[Route("admin/api/upload")]
[ValidateAntiForgeryToken]
public class UploadApiController : Controller
{
    private readonly IFileUploadService _upload;
    private readonly ILogger<UploadApiController> _logger;

    public UploadApiController(IFileUploadService upload, ILogger<UploadApiController> logger)
    {
        _upload = upload;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Index(IFormFile? file, string folder = "products")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Chưa chọn file" });

        try
        {
            var url = await _upload.UploadImageAsync(file, folder);
            if (string.IsNullOrEmpty(url))
                return BadRequest(new { message = "Không thể lưu file" });

            return Ok(new { url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi upload AJAX");
            return StatusCode(500, new { message = "Lỗi hệ thống khi upload" });
        }
    }
}
