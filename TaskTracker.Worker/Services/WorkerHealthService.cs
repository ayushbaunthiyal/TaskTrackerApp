namespace TaskTracker.Worker.Services;

/// <summary>
/// Service to track worker health metrics
/// </summary>
public class WorkerHealthService
{
    private DateTime? _lastSuccessfulRun;
    private int _failedJobsCount;
    private int _totalJobsRun;
    private readonly object _lock = new object();

    public void RecordSuccessfulRun()
    {
        lock (_lock)
        {
            _lastSuccessfulRun = DateTime.UtcNow;
            _totalJobsRun++;
        }
    }

    public void RecordFailedRun()
    {
        lock (_lock)
        {
            _failedJobsCount++;
            _totalJobsRun++;
        }
    }

    public WorkerHealthStatus GetHealthStatus()
    {
        lock (_lock)
        {
            return new WorkerHealthStatus
            {
                LastSuccessfulRun = _lastSuccessfulRun,
                FailedJobsCount = _failedJobsCount,
                TotalJobsRun = _totalJobsRun,
                IsHealthy = _lastSuccessfulRun.HasValue && 
                           (DateTime.UtcNow - _lastSuccessfulRun.Value).TotalMinutes < 120
            };
        }
    }
}

public class WorkerHealthStatus
{
    public DateTime? LastSuccessfulRun { get; set; }
    public int FailedJobsCount { get; set; }
    public int TotalJobsRun { get; set; }
    public bool IsHealthy { get; set; }
}
