using AquaCMS.Helpers;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>Admin CRUD bài viết kiến thức.</summary>
[Area("Admin")]
[Authorize(Policy = "EditorUp")]
public class KnowledgeController : Controller
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly IFileUploadService _upload;
    private readonly IHtmlSanitizerService _sanitizer;
    private readonly IActivityLogService _activity;
    private readonly ILogger<KnowledgeController> _logger;

    public KnowledgeController(
        IKnowledgeService knowledgeService,
        IFileUploadService upload,
        IHtmlSanitizerService sanitizer,
        IActivityLogService activity,
        ILogger<KnowledgeController> logger)
    {
        _knowledgeService = knowledgeService;
        _upload = upload;
        _sanitizer = sanitizer;
        _activity = activity;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Quản lý kiến thức";
        ViewBag.Search = search;
        ViewData["Categories"] = await _knowledgeService.GetCategoriesAsync();
        return View(await _knowledgeService.GetAdminPostsAsync(search, page));
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Thêm bài viết";
        ViewData["Categories"] = await _knowledgeService.GetCategoriesAsync();
        return View(new Post());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Create(Post post, IFormFile? imageFile)
    {
        try
        {
            if (string.IsNullOrEmpty(post.Slug))
                post.Slug = SlugHelper.GenerateSlug(post.Title);

            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "knowledge");
                if (!string.IsNullOrEmpty(url)) post.Image = url;
            }

            // Sanitize HTML content (chống XSS)
            post.Content = _sanitizer.Sanitize(post.Content);

            post.CreatedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;
            if (post.IsPublished && !post.PublishedAt.HasValue)
                post.PublishedAt = DateTime.UtcNow;

            await _knowledgeService.CreateAsync(post);
            await _activity.LogAsync("CREATE", "Post", post.Id.ToString(), post.Title);
            TempData["Success"] = "Đã thêm bài viết thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi tạo bài viết");
            TempData["Error"] = "Không thể tạo bài viết. Vui lòng thử lại.";
        }
        ViewData["Categories"] = await _knowledgeService.GetCategoriesAsync();
        return View(post);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var post = await _knowledgeService.GetByIdAsync(id);
        if (post == null) return NotFound();

        ViewData["Title"] = $"Sửa: {post.Title}";
        ViewData["Categories"] = await _knowledgeService.GetCategoriesAsync();
        return View(post);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Edit(Guid id, Post post, IFormFile? imageFile)
    {
        var existing = await _knowledgeService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        try
        {
            existing.Title = post.Title;
            existing.Slug = string.IsNullOrEmpty(post.Slug) ? SlugHelper.GenerateSlug(post.Title) : post.Slug;
            existing.Excerpt = post.Excerpt;
            existing.Content = _sanitizer.Sanitize(post.Content);
            existing.Author = post.Author;
            existing.KnowledgeCategoryId = post.KnowledgeCategoryId;
            existing.ReadTime = post.ReadTime;
            existing.IsPublished = post.IsPublished;
            existing.MetaTitle = post.MetaTitle;
            existing.MetaDesc = post.MetaDesc;
            existing.UpdatedAt = DateTime.UtcNow;

            if (imageFile is { Length: > 0 })
            {
                var url = await _upload.UploadImageAsync(imageFile, "knowledge");
                if (!string.IsNullOrEmpty(url))
                {
                    _upload.DeleteFile(existing.Image);
                    existing.Image = url;
                }
            }
            else if (!string.IsNullOrEmpty(post.Image))
            {
                existing.Image = post.Image;
            }

            if (post.IsPublished && !existing.PublishedAt.HasValue)
                existing.PublishedAt = DateTime.UtcNow;

            await _knowledgeService.UpdateAsync(existing);
            await _activity.LogAsync("UPDATE", "Post", existing.Id.ToString(), existing.Title);
            TempData["Success"] = "Đã cập nhật bài viết!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật bài viết {Id}", id);
            TempData["Error"] = "Không thể cập nhật bài viết.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _knowledgeService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        try
        {
            _upload.DeleteFile(existing.Image);
            await _knowledgeService.DeleteAsync(id);
            await _activity.LogAsync("DELETE", "Post", id.ToString(), existing.Title);
            TempData["Success"] = "Đã xóa bài viết!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xóa bài viết {Id}", id);
            TempData["Error"] = "Không thể xóa bài viết.";
        }
        return RedirectToAction(nameof(Index));
    }
}
