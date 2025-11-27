# TaskTracker.Worker

Background worker service that sends email reminders for upcoming tasks using Mailgun.

## Features

- **Scheduled Reminders**: Checks for upcoming tasks every 30-60 minutes
- **Email Notifications**: Sends beautiful HTML emails via Mailgun API
- **Idempotency**: Each task gets only ONE reminder (tracked via audit log)
- **Email Quota Management**: Respects Mailgun's 100 emails/day free tier limit
- **Smart Filtering**: Only sends reminders for tasks due within 24 hours
- **Priority-based Sorting**: High-priority tasks are processed first
- **Graceful Shutdown**: Handles cancellation tokens properly

## How It Works

### Reminder Logic

1. **Check Interval**: Worker runs every N minutes (30 in dev, 60 in production)
2. **Lookahead Window**: Finds tasks due within 24 hours
3. **Idempotency Check**: Skips tasks that already have "Reminder sent" audit entry
4. **Priority Sorting**: Processes tasks by due date, then priority (Critical > High > Medium > Low)
5. **Email Quota**: Stops at 90 emails/day (leaves 10 buffer for safety)
6. **Audit Trail**: Logs every reminder sent to prevent duplicates

### Email Content

Each reminder email includes:
- Personalized greeting with username
- Task title and details
- Due date and time (formatted nicely)
- Priority badge with color coding
- Time remaining until due date
- Direct link to TaskTracker UI
- Beautiful responsive HTML design

### Idempotency Guarantee

**Once a reminder is sent for a task, it will NEVER send another reminder for that same task.**

This is achieved by:
1. Creating audit log entry: "Reminder sent for task due {date}"
2. Checking audit logs before sending each reminder
3. Skipping any task that has this audit entry

## Configuration

### appsettings.json (Development)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=tasktracker;Username=postgres;Password=postgres"
  },
  "MailgunSettings": {
    "ApiKey": "your-mailgun-api-key",
    "Domain": "your-mailgun-domain",
    "BaseUrl": "https://api.mailgun.net/v3",
    "FromEmail": "TaskTracker <noreply@yourdomain.com>",
    "FromName": "TaskTracker Reminder Service"
  },
  "WorkerSettings": {
    "CheckIntervalMinutes": 30,
    "DueDateLookaheadHours": 24,
    "MaxEmailsPerRun": 50,
    "DailyEmailQuota": 90,
    "EnableEmailSending": true
  }
}
```

### appsettings.Production.json (Docker)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=tasktracker;Username=postgres;Password=postgres"
  },
  "WorkerSettings": {
    "CheckIntervalMinutes": 60
  }
}
```

### Environment Variables (Docker)

You can override settings using environment variables in docker-compose.yml:

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=tasktracker
  - MailgunSettings__ApiKey=your-api-key
  - WorkerSettings__CheckIntervalMinutes=60
  - WorkerSettings__EnableEmailSending=true
```

## Running Locally

### Prerequisites

1. PostgreSQL running (via Docker or local)
2. TaskTracker.API database migrated
3. Mailgun account with API key

### Run Worker

```powershell
cd d:\repos\mygit\TaskTrackerApp\TaskTracker.Worker
dotnet restore
dotnet run
```

### Test with Sample Tasks

1. Create a user and login
2. Create tasks with due dates in the next 24 hours
3. Wait for worker to process (30 min intervals in dev)
4. Check your email for reminders
5. Verify audit log shows "Reminder sent" entries
6. Verify NO duplicate reminders are sent

### Debug Mode

Set `EnableEmailSending=false` to test without sending actual emails:

```json
"WorkerSettings": {
  "EnableEmailSending": false
}
```

The worker will log "would send" messages instead of actually sending emails.

## Running in Docker

### Build and Run Worker Only

```powershell
docker-compose up -d postgres
docker-compose up --build worker
```

### View Worker Logs

```powershell
docker-compose logs -f worker
```

### Stop Worker

```powershell
docker-compose down worker
```

## Logs

Worker creates logs in two places:

1. **Console**: Real-time logs visible in terminal/Docker logs
2. **File**: `logs/worker-YYYYMMDD.log` (rotates daily)

### Log Levels

- **Information**: Startup, reminder cycles, emails sent
- **Warning**: Email quota warnings, failed sends
- **Error**: Exceptions, database errors
- **Debug**: Detailed processing info (dev only)

### Sample Log Output

```
[14:30:00 INF] Reminder Hosted Service started at 2025-11-27 14:30:00
[14:30:00 INF] Check interval: 30 minutes
[14:30:00 INF] Lookahead window: 24 hours
[14:30:00 INF] Daily email quota: 90
[14:30:10 INF] Database connection successful
[14:30:10 INF] Starting reminder check cycle at 2025-11-27 14:30:10
[14:30:10 INF] Email quota status: 0/90 sent today, max 50 this run
[14:30:11 INF] Found 3 tasks needing reminders
[14:30:12 INF] Email sent successfully to user@example.com for task: Complete Phase 4
[14:30:12 INF] Reminder sent for task 42: Complete Phase 4 to user@example.com
[14:30:13 INF] Reminder processing completed. Sent: 3, Failed: 0, Total today: 3
[14:30:13 INF] Reminder check cycle completed. Next check in 30 minutes.
```

## Email Quota Management

### Free Tier Limits (Mailgun Sandbox)

- **100 emails/day**
- **5 authorized recipients** (must add emails in Mailgun dashboard)

### Worker Safety Features

1. **Daily Quota**: Set to 90/day (10 buffer)
2. **Per-run Limit**: Max 50 emails per cycle
3. **Quota Check**: Counts emails sent today before each run
4. **Automatic Stop**: Stops processing when quota reached
5. **Next-day Reset**: Quota resets at midnight UTC

### Managing Quota

If you hit the limit:

1. **Wait**: Quota resets at midnight UTC
2. **Reduce Frequency**: Increase `CheckIntervalMinutes` to 120-180
3. **Upgrade**: Get Mailgun Flex plan ($35/month for 50k emails)
4. **Switch Provider**: Use SendGrid, AWS SES, or others

## Architecture

```
Program.cs
  ├─ ApplicationDbContext (from Infrastructure)
  ├─ MailgunEmailService (IEmailService)
  ├─ ReminderService (IReminderService)
  └─ ReminderHostedService (BackgroundService)
       └─ ExecuteAsync() - runs every N minutes
            └─ ProcessRemindersAsync()
                 ├─ Check daily quota
                 ├─ Find tasks needing reminders
                 ├─ Check idempotency (audit log)
                 ├─ Send emails
                 └─ Log reminders sent
```

## Testing Checklist

- [ ] Worker starts successfully
- [ ] Connects to PostgreSQL
- [ ] Finds tasks due within 24 hours
- [ ] Sends email via Mailgun
- [ ] Creates audit log entry
- [ ] Does NOT send duplicate reminders
- [ ] Respects daily email quota
- [ ] Handles database errors gracefully
- [ ] Shuts down cleanly on Ctrl+C
- [ ] Works in Docker container

## Troubleshooting

### Worker won't start

- Check PostgreSQL is running
- Verify connection string in appsettings.json
- Check Mailgun API key is correct

### No emails sent

- Check `EnableEmailSending=true`
- Verify Mailgun domain and API key
- Check recipient email is authorized in Mailgun
- Check daily quota not exceeded

### Duplicate emails

- Check audit log table has "Reminder sent" entries
- Verify idempotency logic in ReminderService
- Check database transactions are committing

### Database connection errors

- Verify PostgreSQL is running on correct port
- Check connection string matches your setup
- Ensure database migrations are applied

## Production Deployment

### Best Practices

1. **Use Environment Variables**: Don't hardcode secrets in appsettings.json
2. **Enable HTTPS**: Use SSL for Mailgun API calls
3. **Monitor Logs**: Set up log aggregation (ELK, Seq, etc.)
4. **Health Checks**: Monitor worker uptime
5. **Alerting**: Get notified if worker crashes
6. **Backup Database**: Audit logs are important for idempotency
7. **Scale Horizontally**: Run multiple workers with leader election

### Docker Production

See docker-compose.yml for production configuration:
- Uses Production environment
- Points to `postgres` container hostname
- Sets 60-minute check interval
- Enables restart policy

## Future Enhancements

- [ ] Add retry logic for failed emails
- [ ] Support multiple reminder intervals (1 day, 1 hour, etc.)
- [ ] Add SMS notifications
- [ ] Add push notifications
- [ ] Add user notification preferences
- [ ] Add health check endpoint
- [ ] Add metrics (Prometheus)
- [ ] Add distributed locking for multiple worker instances
- [ ] Add email templates for different task types
- [ ] Add digest emails (daily summary)
