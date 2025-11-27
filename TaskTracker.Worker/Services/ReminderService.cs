using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Data;
using TaskTracker.Worker.Configuration;

namespace TaskTracker.Worker.Services;

public interface IReminderService
{
    Task ProcessRemindersAsync(CancellationToken cancellationToken);
}

public class ReminderService : IReminderService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly WorkerSettings _settings;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        ApplicationDbContext context,
        IEmailService emailService,
        WorkerSettings settings,
        ILogger<ReminderService> logger)
    {
        _context = context;
        _emailService = emailService;
        _settings = settings;
        _logger = logger;
    }

    public async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting reminder processing at {Time}", DateTime.UtcNow);

            // Check daily email quota
            var emailsSentToday = await GetEmailsSentTodayAsync(cancellationToken);
            if (emailsSentToday >= _settings.DailyEmailQuota)
            {
                _logger.LogWarning("Daily email quota reached ({Count}/{Quota}). Skipping reminder processing.",
                    emailsSentToday, _settings.DailyEmailQuota);
                return;
            }

            var remainingQuota = _settings.DailyEmailQuota - emailsSentToday;
            var maxEmailsThisRun = Math.Min(_settings.MaxEmailsPerRun, remainingQuota);

            _logger.LogInformation("Email quota status: {Sent}/{Quota} sent today, max {Max} this run",
                emailsSentToday, _settings.DailyEmailQuota, maxEmailsThisRun);

            // Find tasks that need reminders
            var cutoffDate = DateTime.UtcNow.AddHours(_settings.DueDateLookaheadHours);
            var tasksNeedingReminders = await GetTasksNeedingRemindersAsync(cutoffDate, maxEmailsThisRun, cancellationToken);

            _logger.LogInformation("Found {Count} tasks needing reminders", tasksNeedingReminders.Count);

            var emailsSent = 0;
            var emailsFailed = 0;

            foreach (var task in tasksNeedingReminders)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Cancellation requested, stopping reminder processing");
                    break;
                }

                try
                {
                    // Check if email is enabled
                    if (!_settings.EnableEmailSending)
                    {
                        _logger.LogInformation("Email sending disabled. Would send reminder for task {TaskId}: {Title}",
                            task.Id, task.Title);
                        await LogReminderSentAsync(task, "Email sending disabled (dry run)", cancellationToken);
                        continue;
                    }

                    // Send email
                    var success = await _emailService.SendReminderEmailAsync(
                        task.User.Email,
                        string.IsNullOrEmpty(task.User.FirstName) ? task.User.Email : task.User.FirstName,
                        task.Title,
                        task.DueDate!.Value,
                        task.Priority.ToString()
                    );

                    if (success)
                    {
                        await LogReminderSentAsync(task, "Reminder email sent successfully", cancellationToken);
                        emailsSent++;
                        _logger.LogInformation("Reminder sent for task {TaskId}: {Title} to {Email}",
                            task.Id, task.Title, task.User.Email);
                    }
                    else
                    {
                        emailsFailed++;
                        _logger.LogWarning("Failed to send reminder for task {TaskId}: {Title}",
                            task.Id, task.Title);
                    }

                    // Small delay between emails to avoid rate limiting
                    await Task.Delay(500, cancellationToken);
                }
                catch (Exception ex)
                {
                    emailsFailed++;
                    _logger.LogError(ex, "Error processing reminder for task {TaskId}", task.Id);
                }
            }

            _logger.LogInformation("Reminder processing completed. Sent: {Sent}, Failed: {Failed}, Total today: {Total}",
                emailsSent, emailsFailed, emailsSentToday + emailsSent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in reminder processing");
            throw;
        }
    }

    private async Task<List<TaskItem>> GetTasksNeedingRemindersAsync(
        DateTime cutoffDate,
        int maxTasks,
        CancellationToken cancellationToken)
    {
        // Get tasks that:
        // 1. Have a due date within the lookahead window
        // 2. Are not completed or cancelled
        // 3. Haven't received a reminder yet (no audit entry with "Reminder sent")
        var tasks = await _context.Tasks
            .Include(t => t.User)
            .Where(t => t.DueDate.HasValue &&
                       t.DueDate.Value <= cutoffDate &&
                       t.DueDate.Value > DateTime.UtcNow &&
                       t.Status != Domain.Enums.TaskStatus.Completed &&
                       t.Status != Domain.Enums.TaskStatus.Cancelled)
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .Take(maxTasks * 2) // Get more than needed to filter out those with reminders
            .ToListAsync(cancellationToken);

        // Filter out tasks that already have reminders
        var tasksWithoutReminders = new List<TaskItem>();

        foreach (var task in tasks)
        {
            if (tasksWithoutReminders.Count >= maxTasks)
                break;

            var hasReminder = await _context.AuditLogs
                .AnyAsync(a => a.EntityId == task.Id.ToString() &&
                              a.Action.Contains("Reminder sent"),
                         cancellationToken);

            if (!hasReminder)
            {
                tasksWithoutReminders.Add(task);
            }
        }

        return tasksWithoutReminders;
    }

    private async Task<int> GetEmailsSentTodayAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _context.AuditLogs
            .CountAsync(a => a.Action.Contains("Reminder sent") &&
                           a.Timestamp >= today &&
                           a.Timestamp < tomorrow,
                       cancellationToken);
    }

    private async Task LogReminderSentAsync(TaskItem task, string details, CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog
        {
            EntityId = task.Id.ToString(),
            EntityType = "Task",
            UserId = task.UserId,
            Action = $"Reminder sent for task due {task.DueDate:yyyy-MM-dd HH:mm}",
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
