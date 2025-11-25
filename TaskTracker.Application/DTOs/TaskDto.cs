namespace TaskTracker.Application.DTOs;

public class TaskDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskTracker.Domain.Enums.TaskStatus Status { get; set; }
    public TaskTracker.Domain.Enums.TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
