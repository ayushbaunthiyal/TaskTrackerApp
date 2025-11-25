using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

public class Attachment : BaseEntity
{
    public Guid TaskId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    
    // Navigation property
    public TaskItem Task { get; set; } = null!;
}
