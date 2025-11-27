namespace TaskTracker.Worker.Configuration;

public class WorkerSettings
{
    public int CheckIntervalMinutes { get; set; } = 30;
    public int DueDateLookaheadHours { get; set; } = 24;
    public int MaxEmailsPerRun { get; set; } = 50;
    public int DailyEmailQuota { get; set; } = 90;
    public bool EnableEmailSending { get; set; } = true;
}
