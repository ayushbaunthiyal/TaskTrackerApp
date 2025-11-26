using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces.Services;

public interface IAuditService
{
    Task LogActionAsync(Guid? userId, string action, string entityType, string entityId, string details = "");
    Task<IEnumerable<AuditLogDto>> GetEntityAuditLogsAsync(string entityType, string entityId);
    Task<IEnumerable<AuditLogDto>> GetUserAuditLogsAsync(Guid userId);
}
