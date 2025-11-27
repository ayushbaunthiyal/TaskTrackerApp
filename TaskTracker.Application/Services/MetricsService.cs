using Prometheus;

namespace TaskTracker.Application.Services;

/// <summary>
/// Service for tracking application metrics using Prometheus
/// </summary>
public static class MetricsService
{
    // Counter metrics
    private static readonly Counter TasksCreated = Metrics.CreateCounter(
        "tasktracker_tasks_created_total",
        "Total number of tasks created");

    private static readonly Counter TasksUpdated = Metrics.CreateCounter(
        "tasktracker_tasks_updated_total",
        "Total number of tasks updated");

    private static readonly Counter TasksDeleted = Metrics.CreateCounter(
        "tasktracker_tasks_deleted_total",
        "Total number of tasks deleted");

    private static readonly Counter TasksCompleted = Metrics.CreateCounter(
        "tasktracker_tasks_completed_total",
        "Total number of tasks marked as completed");

    private static readonly Counter AttachmentsUploaded = Metrics.CreateCounter(
        "tasktracker_attachments_uploaded_total",
        "Total number of file attachments uploaded");

    private static readonly Counter AttachmentsDeleted = Metrics.CreateCounter(
        "tasktracker_attachments_deleted_total",
        "Total number of file attachments deleted");

    private static readonly Counter UsersRegistered = Metrics.CreateCounter(
        "tasktracker_users_registered_total",
        "Total number of users registered");

    private static readonly Counter AuthenticationFailures = Metrics.CreateCounter(
        "tasktracker_auth_failures_total",
        "Total number of authentication failures");

    private static readonly Counter AuthenticationSuccesses = Metrics.CreateCounter(
        "tasktracker_auth_successes_total",
        "Total number of successful authentications");

    // Gauge metrics
    private static readonly Gauge ActiveTasks = Metrics.CreateGauge(
        "tasktracker_active_tasks",
        "Current number of active (not completed) tasks");

    // Histogram metrics for latency
    private static readonly Histogram TaskOperationDuration = Metrics.CreateHistogram(
        "tasktracker_task_operation_duration_seconds",
        "Duration of task operations in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
        });

    // Methods to increment counters
    public static void RecordTaskCreated() => TasksCreated.Inc();
    public static void RecordTaskUpdated() => TasksUpdated.Inc();
    public static void RecordTaskDeleted() => TasksDeleted.Inc();
    public static void RecordTaskCompleted() => TasksCompleted.Inc();
    public static void RecordAttachmentUploaded() => AttachmentsUploaded.Inc();
    public static void RecordAttachmentDeleted() => AttachmentsDeleted.Inc();
    public static void RecordUserRegistered() => UsersRegistered.Inc();
    public static void RecordAuthenticationFailure() => AuthenticationFailures.Inc();
    public static void RecordAuthenticationSuccess() => AuthenticationSuccesses.Inc();

    // Methods to update gauges
    public static void SetActiveTasks(int count) => ActiveTasks.Set(count);

    // Methods to record durations
    public static IDisposable MeasureTaskOperation() => TaskOperationDuration.NewTimer();
}
