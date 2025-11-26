using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Application.Interfaces.Services;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogActionAsync(Guid? userId, string action, string entityType, string entityId, string details = "")
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            Details = details
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLogDto>> GetEntityAuditLogsAsync(string entityType, string entityId)
    {
        var logs = await _unitOfWork.AuditLogs.GetByEntityAsync(entityType, entityId);
        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<AuditLogDto>> GetUserAuditLogsAsync(Guid userId)
    {
        var logs = await _unitOfWork.AuditLogs.GetByUserIdAsync(userId);
        return logs.Select(MapToDto);
    }

    private static AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Timestamp = log.Timestamp,
            Details = log.Details,
            UserEmail = log.User?.Email
        };
    }
}
