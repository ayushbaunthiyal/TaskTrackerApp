using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Application.Interfaces.Services;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public TaskService(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<PaginatedResponse<TaskDto>> GetFilteredTasksAsync(TaskFilterDto filter)
    {
        // Ensure page size doesn't exceed maximum
        filter.PageSize = Math.Min(filter.PageSize, 100);
        filter.PageSize = Math.Max(filter.PageSize, 1);
        filter.PageNumber = Math.Max(filter.PageNumber, 1);

        var (items, totalCount) = await _unitOfWork.Tasks.GetFilteredTasksAsync(
            filter.SearchTerm,
            (int?)filter.Status,
            (int?)filter.Priority,
            filter.Tag,
            filter.DueDateFrom,
            filter.DueDateTo,
            filter.SortBy,
            filter.SortDescending,
            filter.PageNumber,
            filter.PageSize
        );

        var taskDtos = items.Select(MapToDto).ToList();

        return new PaginatedResponse<TaskDto>
        {
            Items = taskDtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid id)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        return task == null ? null : MapToDto(task);
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = createTaskDto.UserId,
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Status = createTaskDto.Status,
            Priority = createTaskDto.Priority,
            DueDate = createTaskDto.DueDate,
            Tags = createTaskDto.Tags,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Tasks.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogActionAsync(task.UserId, "Create", "Task", task.Id.ToString(), 
            $"Created task: {task.Title}");

        return MapToDto(task);
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {id} not found");
        }

        task.Title = updateTaskDto.Title;
        task.Description = updateTaskDto.Description;
        task.Status = updateTaskDto.Status;
        task.Priority = updateTaskDto.Priority;
        task.DueDate = updateTaskDto.DueDate;
        task.Tags = updateTaskDto.Tags;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogActionAsync(task.UserId, "Update", "Task", task.Id.ToString(), 
            $"Updated task: {task.Title}");

        return MapToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (task == null)
        {
            return false;
        }

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogActionAsync(task.UserId, "Delete", "Task", task.Id.ToString(), 
            $"Deleted task: {task.Title}");

        return true;
    }

    private static TaskDto MapToDto(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            UserId = task.UserId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            Tags = task.Tags,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
