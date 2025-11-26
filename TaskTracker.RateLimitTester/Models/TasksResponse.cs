namespace TaskTracker.RateLimitTester.Models;

public class TasksResponse
{
    public List<TaskItem>? Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class TaskItem
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
