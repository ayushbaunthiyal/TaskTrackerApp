using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces.Repositories;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetFilteredTasksAsync(
        string? searchTerm,
        int? status,
        int? priority,
        string? tag,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        string sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize);
}
