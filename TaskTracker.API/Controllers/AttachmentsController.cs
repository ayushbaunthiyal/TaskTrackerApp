using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("PerUserPolicy")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(
        IAttachmentService attachmentService,
        ILogger<AttachmentsController> logger)
    {
        _attachmentService = attachmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all attachments for a specific task
    /// </summary>
    [HttpGet("task/{taskId}")]
    [ProducesResponseType(typeof(IEnumerable<AttachmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetTaskAttachments(Guid taskId)
    {
        var attachments = await _attachmentService.GetTaskAttachmentsAsync(taskId);
        return Ok(attachments);
    }

    /// <summary>
    /// Upload a new attachment to a task
    /// </summary>
    [HttpPost("task/{taskId}")]
    [EnableRateLimiting("PerIpStrictPolicy")]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttachmentDto>> UploadAttachment(Guid taskId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        // Limit file size to 10MB
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { error = "File size must be less than 10MB" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var attachment = await _attachmentService.UploadAttachmentAsync(
                taskId,
                stream,
                file.FileName,
                file.Length);

            return CreatedAtAction(nameof(DownloadAttachment), new { id = attachment.Id }, attachment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Download an attachment
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAttachment(Guid id)
    {
        try
        {
            var (fileStream, fileName, contentType) = await _attachmentService.DownloadAttachmentAsync(id);
            return File(fileStream, contentType, fileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an attachment
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttachment(Guid id)
    {
        try
        {
            await _attachmentService.DeleteAttachmentAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
