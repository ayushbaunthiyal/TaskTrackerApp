using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces;

public interface IAttachmentService
{
    Task<IEnumerable<AttachmentDto>> GetTaskAttachmentsAsync(Guid taskId);
    Task<AttachmentDto> UploadAttachmentAsync(Guid taskId, Stream fileStream, string fileName, long fileSize);
    Task<(Stream FileStream, string FileName, string ContentType)> DownloadAttachmentAsync(Guid attachmentId);
    Task DeleteAttachmentAsync(Guid attachmentId);
}
