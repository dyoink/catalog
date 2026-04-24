using System.Security.Claims;
using AquaCMS.Data;
using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;
using AquaCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaCMS.Services;

/// <summary>
/// Implementation activity log — đọc thông tin user/IP từ HttpContext.
/// </summary>
public class ActivityLogService : IActivityLogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(AppDbContext db, IHttpContextAccessor http, ILogger<ActivityLogService> logger)
    {
        _db = db;
        _http = http;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityType, string? entityId = null, string? description = null, string severity = "Info")
    {
        try
        {
            var ctx = _http.HttpContext;
            var user = ctx?.User;

            var userIdStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;
            var userName = user?.FindFirst(ClaimTypes.Name)?.Value
                           ?? user?.FindFirst(ClaimTypes.Email)?.Value
                           ?? "system";

            // Auto-infer severity nếu caller không truyền
            if (string.IsNullOrWhiteSpace(severity) || severity == "Info")
            {
                severity = action.ToUpperInvariant() switch
                {
                    "DELETE" or "BULK_DELETE" => "Warning",
                    "ERROR" or "FAIL" or "EXCEPTION" => "Error",
                    "CREATE" or "UPDATE" or "IMPORT" or "EXPORT" or "LOGIN" or "CHANGE_PASSWORD" => "Success",
                    _ => severity
                };
            }

            var log = new ActivityLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                IpAddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = ctx?.Request.Headers.UserAgent.ToString(),
                Severity = severity
            };

            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Không throw — activity log không nên crash flow chính
            _logger.LogError(ex, "Lỗi ghi activity log: {Action} {Entity}", action, entityType);
        }
    }

    public async Task<PaginatedList<ActivityLog>> GetLogsAsync(int page = 1, int pageSize = 30,
        string? entityType = null, Guid? userId = null,
        string? action = null, string? severity = null, string? search = null)
    {
        var query = _db.ActivityLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(l => l.EntityType == entityType);

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action == action);

        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(l => l.Severity == severity);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search}%";
            query = query.Where(l =>
                EF.Functions.ILike(l.UserName, s) ||
                (l.Description != null && EF.Functions.ILike(l.Description, s)) ||
                (l.EntityId != null && EF.Functions.ILike(l.EntityId, s)));
        }

        query = query.OrderByDescending(l => l.CreatedAt);
        return await PaginatedList<ActivityLog>.CreateAsync(query, page, pageSize);
    }
}
