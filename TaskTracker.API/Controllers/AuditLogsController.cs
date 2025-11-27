using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Application.Interfaces.Services;

namespace TaskTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("PerUserPolicy")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditService auditService, 
        IAuditLogRepository auditLogRepository,
        ILogger<AuditLogsController> logger)
    {
        _auditService = auditService;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all audit logs with optional filtering, sorting, and pagination
    /// Available to all authenticated users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<AuditLogListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<AuditLogListDto>>> GetAuditLogs(
        [FromQuery] string? searchTerm,
        [FromQuery] string? userEmail,
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string sortBy = "Timestamp",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25)
    {
        try
        {
            var filter = new AuditLogFilterDto
            {
                SearchTerm = searchTerm,
                UserEmail = userEmail,
                EntityType = entityType,
                Action = action,
                DateFrom = dateFrom,
                DateTo = dateTo,
                SortBy = sortBy,
                SortDescending = sortDescending,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var (items, totalCount) = await _auditLogRepository.GetFilteredAuditLogsAsync(filter);

            var response = new PaginatedResponse<AuditLogListDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, "An error occurred while retrieving audit logs");
        }
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
