using Prometheus;

namespace TaskTracker.Worker.Services;

/// <summary>
/// Service for tracking worker metrics using Prometheus
/// </summary>
public static class WorkerMetricsService
{
    // Counter metrics
    private static readonly Counter RemindersProcessed = Metrics.CreateCounter(
        "tasktracker_worker_reminders_processed_total",
        "Total number of reminder emails processed");

    private static readonly Counter RemindersSent = Metrics.CreateCounter(
        "tasktracker_worker_reminders_sent_total",
        "Total number of reminder emails sent successfully");

    private static readonly Counter RemindersFailed = Metrics.CreateCounter(
        "tasktracker_worker_reminders_failed_total",
        "Total number of reminder emails that failed to send");

    private static readonly Counter RemindersSkipped = Metrics.CreateCounter(
        "tasktracker_worker_reminders_skipped_total",
        "Total number of reminders skipped (already sent)");

    private static readonly Counter WorkerCycles = Metrics.CreateCounter(
        "tasktracker_worker_cycles_total",
        "Total number of worker check cycles completed");

    private static readonly Counter WorkerErrors = Metrics.CreateCounter(
        "tasktracker_worker_errors_total",
        "Total number of worker errors encountered");

    // Gauge metrics
    private static readonly Gauge LastRunTimestamp = Metrics.CreateGauge(
        "tasktracker_worker_last_run_timestamp",
        "Unix timestamp of last successful worker run");

    private static readonly Gauge EmailsRemainingToday = Metrics.CreateGauge(
        "tasktracker_worker_emails_remaining_today",
        "Number of emails remaining in today's quota");

    private static readonly Gauge TasksDueWithin24Hours = Metrics.CreateGauge(
        "tasktracker_worker_tasks_due_within_24hours",
        "Current number of tasks due within 24 hours");

    // Histogram for cycle duration
    private static readonly Histogram WorkerCycleDuration = Metrics.CreateHistogram(
        "tasktracker_worker_cycle_duration_seconds",
        "Duration of worker check cycles in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.1, 2, 10)
        });

    // Methods to increment counters
    public static void RecordReminderProcessed() => RemindersProcessed.Inc();
    public static void RecordReminderSent() => RemindersSent.Inc();
    public static void RecordReminderFailed() => RemindersFailed.Inc();
    public static void RecordReminderSkipped() => RemindersSkipped.Inc();
    public static void RecordWorkerCycle() => WorkerCycles.Inc();
    public static void RecordWorkerError() => WorkerErrors.Inc();

    // Methods to update gauges
    public static void SetLastRunTimestamp() => LastRunTimestamp.SetToCurrentTimeUtc();
    public static void SetEmailsRemainingToday(int count) => EmailsRemainingToday.Set(count);
    public static void SetTasksDueWithin24Hours(int count) => TasksDueWithin24Hours.Set(count);

    // Methods to record durations
    public static IDisposable MeasureWorkerCycle() => WorkerCycleDuration.NewTimer();
}
