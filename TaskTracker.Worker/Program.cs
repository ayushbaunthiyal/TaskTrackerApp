using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using TaskTracker.Infrastructure.Data;
using TaskTracker.Worker.Configuration;
using TaskTracker.Worker.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting TaskTracker Worker Service");

    var builder = Host.CreateApplicationBuilder(args);

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
    builder.Services.AddScoped<IEmailService, MailgunEmailService>();
    builder.Services.AddScoped<IReminderService, ReminderService>();

    // Add Hosted Service
    builder.Services.AddHostedService<ReminderHostedService>();

    var host = builder.Build();

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
