using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces.Services;

public interface IAuditService
{
    Task LogActionAsync(Guid? userId, string action, string entityType, string entityId, string details = "");
}
