using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetFilteredTasksAsync(
        string? searchTerm,
        int? status,
        int? priority,
        string? tag,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        string sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize)
    {
        var query = _dbSet.Include(t => t.Attachments).Include(t => t.User).AsQueryable();

        // Apply filters (case-insensitive search)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t =>
                EF.Functions.ILike(t.Title, $"%{searchTerm}%") ||
                EF.Functions.ILike(t.Description, $"%{searchTerm}%"));
        }

        if (status.HasValue)
        {
            query = query.Where(t => (int)t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => (int)t.Priority == priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            // PostgreSQL JSONB array search with exact match
            query = query.Where(t => EF.Functions.JsonContains(t.Tags, $"\"{tag}\""));
        }

        if (dueDateFrom.HasValue)
        {
            var dueDateFromUtc = DateTime.SpecifyKind(dueDateFrom.Value, DateTimeKind.Utc);
            query = query.Where(t => t.DueDate >= dueDateFromUtc);
        }

        if (dueDateTo.HasValue)
        {
            var dueDateToUtc = DateTime.SpecifyKind(dueDateTo.Value, DateTimeKind.Utc);
            query = query.Where(t => t.DueDate <= dueDateToUtc);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "status" => sortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => sortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "duedate" => sortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "updatedat" => sortDescending ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
            _ => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
