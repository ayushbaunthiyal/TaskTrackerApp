namespace TaskTracker.Application.DTOs;

public class TaskFilterDto
{
    public string? SearchTerm { get; set; }
    public TaskTracker.Domain.Enums.TaskStatus? Status { get; set; }
    public TaskTracker.Domain.Enums.TaskPriority? Priority { get; set; }
    public string? Tag { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
