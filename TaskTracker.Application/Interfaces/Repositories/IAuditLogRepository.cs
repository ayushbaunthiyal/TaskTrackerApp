using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId);
    Task<(IEnumerable<AuditLogListDto> Items, int TotalCount)> GetFilteredAuditLogsAsync(AuditLogFilterDto filter);
}
