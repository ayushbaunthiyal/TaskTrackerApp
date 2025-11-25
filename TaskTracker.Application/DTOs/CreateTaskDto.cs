namespace TaskTracker.Application.DTOs;

public class CreateTaskDto
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskTracker.Domain.Enums.TaskStatus Status { get; set; } = TaskTracker.Domain.Enums.TaskStatus.Pending;
    public TaskTracker.Domain.Enums.TaskPriority Priority { get; set; } = TaskTracker.Domain.Enums.TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}
