using AquaCMS.Data;
using AquaCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Areas.Admin.Controllers;

/// <summary>Trang xem activity log (audit trail). Manager+ truy cập.</summary>
[Area("Admin")]
[Authorize(Policy = "ManagerUp")]
public class ActivityController : Controller
{
    private readonly IActivityLogService _activity;
    private readonly AppDbContext _db;

    public ActivityController(IActivityLogService activity, AppDbContext db)
    {
        _activity = activity;
        _db = db;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        string? entityType = null,
        string? logAction = null,
        string? severity = null,
        string? search = null)
    {
        ViewData["Title"] = "Nhật ký hoạt động";
        
        // Chuyển chuỗi rỗng thành null để Service bỏ qua bộ lọc
        entityType = string.IsNullOrWhiteSpace(entityType) ? null : entityType;
        logAction = string.IsNullOrWhiteSpace(logAction) ? null : logAction;
        severity = string.IsNullOrWhiteSpace(severity) ? null : severity;
        search = string.IsNullOrWhiteSpace(search) ? null : search;

        ViewBag.EntityType = entityType;
        ViewBag.Action = logAction;
        ViewBag.Severity = severity;
        ViewBag.Search = search;

        // Stats cards - lấy trực tiếp từ DB
        ViewBag.CountTotal = await _db.ActivityLogs.CountAsync();
        ViewBag.CountInfo = await _db.ActivityLogs.CountAsync(l => l.Severity == "Info" || string.IsNullOrEmpty(l.Severity));
        ViewBag.CountSuccess = await _db.ActivityLogs.CountAsync(l => l.Severity == "Success");
        ViewBag.CountWarning = await _db.ActivityLogs.CountAsync(l => l.Severity == "Warning");
        ViewBag.CountError = await _db.ActivityLogs.CountAsync(l => l.Severity == "Error");

        // Lấy danh sách log (kèm phân trang)
        var logs = await _activity.GetLogsAsync(page, 30, entityType, null, logAction, severity, search);
        return View(logs);
    }
}
