using TaskTracker.Worker.Configuration;

namespace TaskTracker.Worker.Services;

public class ReminderHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly WorkerSettings _settings;
    private readonly ILogger<ReminderHostedService> _logger;
    private readonly WorkerHealthService _healthService;

    public ReminderHostedService(
        IServiceProvider serviceProvider,
        WorkerSettings settings,
        ILogger<ReminderHostedService> logger,
        WorkerHealthService healthService)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
        _logger = logger;
        _healthService = healthService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder Hosted Service started at {Time}", DateTime.UtcNow);
        _logger.LogInformation("Check interval: {Interval} minutes", _settings.CheckIntervalMinutes);
        _logger.LogInformation("Lookahead window: {Hours} hours", _settings.DueDateLookaheadHours);
        _logger.LogInformation("Daily email quota: {Quota}", _settings.DailyEmailQuota);

        // Wait 10 seconds before first run to allow services to initialize
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting reminder check cycle at {Time}", DateTime.UtcNow);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                    await reminderService.ProcessRemindersAsync(stoppingToken);
                }

                _healthService.RecordSuccessfulRun();
                
                _logger.LogInformation("Reminder check cycle completed. Next check in {Minutes} minutes.",
                    _settings.CheckIntervalMinutes);
            }
            catch (Exception ex)
            {
                _healthService.RecordFailedRun();
                _logger.LogError(ex, "Error occurred during reminder processing cycle");
            }

            // Wait for the configured interval before next check
            await Task.Delay(TimeSpan.FromMinutes(_settings.CheckIntervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Reminder Hosted Service stopped at {Time}", DateTime.UtcNow);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reminder Hosted Service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}
