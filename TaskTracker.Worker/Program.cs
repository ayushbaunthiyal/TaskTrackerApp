using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using TaskTracker.Infrastructure.Data;
using TaskTracker.Worker.Configuration;
using TaskTracker.Worker.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Configure Serilog from appsettings.json
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    Log.Information("Starting TaskTracker Worker Service");

    // Add Serilog
    builder.Services.AddSerilog();

    // Load configuration
    var mailgunSettings = builder.Configuration.GetSection("MailgunSettings").Get<MailgunSettings>()
        ?? throw new InvalidOperationException("MailgunSettings not configured");
    var workerSettings = builder.Configuration.GetSection("WorkerSettings").Get<WorkerSettings>()
        ?? throw new InvalidOperationException("WorkerSettings not configured");

    builder.Services.AddSingleton(mailgunSettings);
    builder.Services.AddSingleton(workerSettings);

    // Add Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

    // Configure Npgsql for JSON serialization (required for Tags field)
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(dataSource));

    // Add Services
    builder.Services.AddSingleton<WorkerHealthService>();
    builder.Services.AddScoped<IEmailService, MailgunEmailService>();
    builder.Services.AddScoped<IReminderService, ReminderService>();

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database")
        .AddCheck("worker_health", () =>
        {
            var healthService = builder.Services.BuildServiceProvider().GetService<WorkerHealthService>();
            if (healthService == null)
                return HealthCheckResult.Unhealthy("WorkerHealthService not initialized");
            
            var status = healthService.GetHealthStatus();
            return status.IsHealthy
                ? HealthCheckResult.Healthy($"Last run: {status.LastSuccessfulRun:yyyy-MM-dd HH:mm:ss}, Failed jobs: {status.FailedJobsCount}")
                : HealthCheckResult.Unhealthy($"Worker hasn't run successfully in over 2 hours. Last run: {status.LastSuccessfulRun:yyyy-MM-dd HH:mm:ss}");
        });
    
    // Add Hosted Service
    builder.Services.AddHostedService<ReminderHostedService>();

    var host = builder.Build();

    // Configure HTTP endpoints for health checks and metrics
    var webAppBuilder = WebApplication.CreateBuilder(args);
    webAppBuilder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database")
        .AddCheck("worker_health", () =>
        {
            var healthService = host.Services.GetService<WorkerHealthService>();
            if (healthService == null)
                return HealthCheckResult.Unhealthy("WorkerHealthService not initialized");
            
            var status = healthService.GetHealthStatus();
            return status.IsHealthy
                ? HealthCheckResult.Healthy($"Last run: {status.LastSuccessfulRun:yyyy-MM-dd HH:mm:ss UTC}, Failed jobs: {status.FailedJobsCount}, Total runs: {status.TotalJobsRun}")
                : HealthCheckResult.Unhealthy($"Worker hasn't run successfully in over 2 hours. Last run: {status.LastSuccessfulRun:yyyy-MM-dd HH:mm:ss UTC}");
        });
    webAppBuilder.Services.AddSingleton(host.Services.GetRequiredService<ApplicationDbContext>());
    webAppBuilder.Services.AddSingleton(host.Services.GetRequiredService<WorkerHealthService>());
    
    var webApp = webAppBuilder.Build();
    
    // Health check endpoint
    webApp.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    });
    
    // Metrics endpoint
    webApp.MapMetrics();
    
    webApp.Urls.Add("http://localhost:5129"); // Different port from API
    
    // Start web server in background
    _ = webApp.RunAsync();
    
    Log.Information("Worker health endpoint available at http://localhost:5129/health");
    Log.Information("Worker metrics endpoint available at http://localhost:5129/metrics");

    // Verify database connection on startup
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync();
            
            if (canConnect)
            {
                Log.Information("Database connection successful");
                
                // Verify tables exist
                var tablesExist = await dbContext.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Tasks\" LIMIT 1") >= 0;
                Log.Information("Database tables verified");
            }
            else
            {
                Log.Error("Cannot connect to database - CanConnectAsync returned false");
                throw new InvalidOperationException("Cannot connect to database");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database connection error. Connection string: {ConnectionString}", 
                connectionString.Replace(builder.Configuration.GetConnectionString("DefaultConnection")!.Split("Password=")[1], "***"));
            throw new InvalidOperationException("Cannot connect to database", ex);
        }
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
