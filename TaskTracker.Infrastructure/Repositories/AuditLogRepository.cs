using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        return await _dbSet
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<(IEnumerable<AuditLogListDto> Items, int TotalCount)> GetFilteredAuditLogsAsync(AuditLogFilterDto filter)
    {
        var query = _dbSet
            .Include(a => a.User)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Action.ToLower().Contains(searchLower) ||
                a.Details.ToLower().Contains(searchLower) ||
                (a.User != null && (a.User.FirstName.ToLower().Contains(searchLower) ||
                                   a.User.LastName.ToLower().Contains(searchLower) ||
                                   a.User.Email.ToLower().Contains(searchLower))));
        }

        if (!string.IsNullOrWhiteSpace(filter.UserEmail))
        {
            query = query.Where(a => a.User != null && a.User.Email == filter.UserEmail);
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
        {
            query = query.Where(a => a.EntityType == filter.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(a => a.Action == filter.Action);
        }

        if (filter.DateFrom.HasValue)
        {
            // Convert to UTC if not already
            var dateFrom = filter.DateFrom.Value.Kind == DateTimeKind.Utc 
                ? filter.DateFrom.Value 
                : DateTime.SpecifyKind(filter.DateFrom.Value, DateTimeKind.Utc);
            query = query.Where(a => a.Timestamp >= dateFrom);
        }

        if (filter.DateTo.HasValue)
        {
            // Include the entire day and convert to UTC if not already
            var dateTo = filter.DateTo.Value.Date.AddDays(1).AddTicks(-1);
            dateTo = dateTo.Kind == DateTimeKind.Utc 
                ? dateTo 
                : DateTime.SpecifyKind(dateTo, DateTimeKind.Utc);
            query = query.Where(a => a.Timestamp <= dateTo);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = filter.SortBy.ToLower() switch
        {
            "action" => filter.SortDescending
                ? query.OrderByDescending(a => a.Action)
                : query.OrderBy(a => a.Action),
            "entitytype" => filter.SortDescending
                ? query.OrderByDescending(a => a.EntityType)
                : query.OrderBy(a => a.EntityType),
            "useremail" => filter.SortDescending
                ? query.OrderByDescending(a => a.User != null ? a.User.Email : "")
                : query.OrderBy(a => a.User != null ? a.User.Email : ""),
            _ => filter.SortDescending
                ? query.OrderByDescending(a => a.Timestamp)
                : query.OrderBy(a => a.Timestamp)
        };

        // Apply pagination
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AuditLogListDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : null,
                UserEmail = a.User != null ? a.User.Email : null,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Timestamp = a.Timestamp,
                Details = a.Details
            })
            .ToListAsync();

        return (items, totalCount);
    }
}
