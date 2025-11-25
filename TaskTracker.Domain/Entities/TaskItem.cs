using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

public class TaskItem : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; }
    public Enums.TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
