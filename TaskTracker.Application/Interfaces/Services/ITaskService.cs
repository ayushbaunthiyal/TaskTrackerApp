using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces.Services;

public interface ITaskService
{
    Task<PaginatedResponse<TaskDto>> GetFilteredTasksAsync(TaskFilterDto filter);
    Task<TaskDto?> GetTaskByIdAsync(Guid id);
    Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto);
    Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto);
    Task<bool> DeleteTaskAsync(Guid id);
}
