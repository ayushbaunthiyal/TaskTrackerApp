using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Application.Interfaces.Services;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public TaskService(IUnitOfWork unitOfWork, IAuditService auditService, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
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
        // Get userId from JWT token
        var userId = _currentUserService.UserId;
        
        // Validate that the user exists
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
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

        await _auditService.LogActionAsync(task.UserId, "Created", "TaskItem", task.Id.ToString(), 
            $"Created task: {task.Title}");

        return MapToDto(task);
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        
        // Ownership validation: Check ownership before revealing if task exists (security best practice)
        var currentUserId = _currentUserService.UserId;
        if (task != null && task.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only modify your own tasks");
        }
        
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {id} not found");
        }

        // Track changes for audit log
        var changes = new List<string>();
        if (task.Title != updateTaskDto.Title) changes.Add($"Title: '{task.Title}' → '{updateTaskDto.Title}'");
        if (task.Description != updateTaskDto.Description) changes.Add($"Description changed");
        if (task.Status != updateTaskDto.Status) changes.Add($"Status: {task.Status} → {updateTaskDto.Status}");
        if (task.Priority != updateTaskDto.Priority) changes.Add($"Priority: {task.Priority} → {updateTaskDto.Priority}");
        if (task.DueDate != updateTaskDto.DueDate) changes.Add($"DueDate: {task.DueDate} → {updateTaskDto.DueDate}");

        task.Title = updateTaskDto.Title;
        task.Description = updateTaskDto.Description;
        task.Status = updateTaskDto.Status;
        task.Priority = updateTaskDto.Priority;
        task.DueDate = updateTaskDto.DueDate;
        task.Tags = updateTaskDto.Tags;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        var changeDetails = changes.Any() ? string.Join("; ", changes) : "No changes";
        await _auditService.LogActionAsync(currentUserId, "Updated", "TaskItem", task.Id.ToString(), 
            changeDetails);

        return MapToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        
        // Ownership validation: Check ownership before revealing if task exists (security best practice)
        var currentUserId = _currentUserService.UserId;
        if (task != null && task.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only modify your own tasks");
        }
        
        if (task == null)
        {
            return false;
        }

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogActionAsync(currentUserId, "Deleted", "TaskItem", task.Id.ToString(), 
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
