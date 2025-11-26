using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Application.Interfaces.Services;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly string _uploadPath;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService)
    {
        _attachmentRepository = attachmentRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        
        // Store files in Uploads folder
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<IEnumerable<AttachmentDto>> GetTaskAttachmentsAsync(Guid taskId)
    {
        var attachments = await _attachmentRepository.GetByTaskIdAsync(taskId);
        return attachments.Select(a => new AttachmentDto
        {
            Id = a.Id,
            TaskId = a.TaskId,
            FileName = a.FileName,
            FileSize = a.FileSize,
            UploadedAt = a.UploadedAt
        });
    }

    public async Task<AttachmentDto> UploadAttachmentAsync(Guid taskId, Stream fileStream, string fileName, long fileSize)
    {
        // Verify task exists
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        // Verify user owns the task
        if (task.UserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You can only upload attachments to your own tasks");
        }

        // Generate unique file name
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);

        // Save file to disk
        using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOut);
        }

        // Create attachment record
        var attachment = new Attachment
        {
            TaskId = taskId,
            FileName = fileName,
            FileSize = fileSize,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow
        };

        await _attachmentRepository.AddAsync(attachment);
        await _unitOfWork.SaveChangesAsync();

        // Log audit event
        await _auditService.LogActionAsync(_currentUserService.UserId, "AttachmentUploaded", "Attachment", 
            attachment.Id.ToString(), $"Uploaded file: {fileName} ({fileSize} bytes) to task {taskId}");

        return new AttachmentDto
        {
            Id = attachment.Id,
            TaskId = attachment.TaskId,
            FileName = attachment.FileName,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt
        };
    }

    public async Task<(Stream FileStream, string FileName, string ContentType)> DownloadAttachmentAsync(Guid attachmentId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
        if (attachment == null)
        {
            throw new KeyNotFoundException($"Attachment with ID {attachmentId} not found");
        }

        if (!File.Exists(attachment.FilePath))
        {
            throw new FileNotFoundException("File not found on disk");
        }

        var fileStream = new FileStream(attachment.FilePath, FileMode.Open, FileAccess.Read);
        var contentType = GetContentType(attachment.FileName);

        return (fileStream, attachment.FileName, contentType);
    }

    public async Task DeleteAttachmentAsync(Guid attachmentId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
        if (attachment == null)
        {
            throw new KeyNotFoundException($"Attachment with ID {attachmentId} not found");
        }

        // Verify user owns the task
        var task = await _taskRepository.GetByIdAsync(attachment.TaskId);
        if (task == null || task.UserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You can only delete attachments from your own tasks");
        }

        // Delete file from disk
        if (File.Exists(attachment.FilePath))
        {
            File.Delete(attachment.FilePath);
        }

        // Delete database record
        await _attachmentRepository.DeleteAsync(attachment);
        await _unitOfWork.SaveChangesAsync();

        // Log audit event
        await _auditService.LogActionAsync(_currentUserService.UserId, "AttachmentDeleted", "Attachment", 
            attachmentId.ToString(), $"Deleted file: {attachment.FileName} from task {attachment.TaskId}");
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
