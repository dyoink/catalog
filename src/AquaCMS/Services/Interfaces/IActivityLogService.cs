using AquaCMS.Models.Common;
using AquaCMS.Models.Entities;

namespace AquaCMS.Services.Interfaces;

/// <summary>
/// Service ghi log hoạt động admin (audit trail).
/// </summary>
public interface IActivityLogService
{
    /// <summary>Ghi 1 hoạt động — fire & forget, không throw exception</summary>
    Task LogAsync(string action, string entityType, string? entityId = null, string? description = null, string severity = "Info");

    /// <summary>Lấy logs cho admin viewer (phân trang) + filter theo entity, user, action, severity, search.</summary>
    Task<PaginatedList<ActivityLog>> GetLogsAsync(int page = 1, int pageSize = 30,
        string? entityType = null, Guid? userId = null,
        string? action = null, string? severity = null, string? search = null);
}
