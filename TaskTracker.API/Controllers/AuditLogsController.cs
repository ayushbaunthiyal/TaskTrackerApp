using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces.Services;

namespace TaskTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("task/{taskId}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetTaskAuditLogs(Guid taskId)
    {
        var logs = await _auditService.GetEntityAuditLogsAsync("TaskItem", taskId.ToString());
        return Ok(logs);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetUserAuditLogs(Guid userId)
    {
        var logs = await _auditService.GetUserAuditLogsAsync(userId);
        return Ok(logs);
    }
}
