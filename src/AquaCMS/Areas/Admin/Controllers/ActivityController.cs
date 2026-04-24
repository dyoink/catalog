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
        string? action = null,
        string? severity = null,
        string? search = null)
    {
        ViewData["Title"] = "Nhật ký hoạt động";
        ViewBag.EntityType = entityType;
        ViewBag.Action = action;
        ViewBag.Severity = severity;
        ViewBag.Search = search;

        // Aggregate stats cho mấy chip ở đầu trang
        var statsQuery = _db.ActivityLogs.AsNoTracking();
        ViewBag.CountTotal = await statsQuery.CountAsync();
        ViewBag.CountInfo = await statsQuery.CountAsync(l => l.Severity == "Info" || l.Severity == null);
        ViewBag.CountSuccess = await statsQuery.CountAsync(l => l.Severity == "Success");
        ViewBag.CountWarning = await statsQuery.CountAsync(l => l.Severity == "Warning");
        ViewBag.CountError = await statsQuery.CountAsync(l => l.Severity == "Error");

        var logs = await _activity.GetLogsAsync(page, 30, entityType, null, action, severity, search);
        return View(logs);
    }
}
