# TaskTracker Worker - Quick Start Guide

## Step 1: Ensure PostgreSQL is Running

```powershell
# If using Docker
docker-compose up -d postgres

# Wait 10 seconds for PostgreSQL to be ready
Start-Sleep -Seconds 10

# Verify PostgreSQL is running
docker ps | Select-String "tasktracker-postgres"
```

## Step 2: Restore Dependencies

```powershell
cd d:\repos\mygit\TaskTrackerApp\TaskTracker.Worker
dotnet restore
```

## Step 3: Build the Worker

```powershell
dotnet build
```

## Step 4: Run the Worker

```powershell
# Run in development mode (30-minute intervals)
dotnet run

# OR run with specific environment
dotnet run --environment Production
```

## Step 5: Create Test Tasks

Open a new PowerShell window and:

1. Start the API (if not already running):
```powershell
cd d:\repos\mygit\TaskTrackerApp\TaskTracker.API
dotnet run
```

2. Start the React UI (if not already running):
```powershell
cd d:\repos\mygit\TaskTrackerApp\task-tracker-ui
npm start
```

3. Open browser: http://localhost:3000
4. Login or register a new user
5. **IMPORTANT**: Add your email to Mailgun authorized recipients:
   - Go to: https://app.mailgun.com/app/sending/domains/sandboxe08c31c1424440319890bc2272863d49.mailgun.org
   - Click "Authorized Recipients"
   - Add your email address
   - Verify the email you receive

6. Create tasks with due dates in the next 24 hours:
   - Task 1: Due in 2 hours (HIGH priority)
   - Task 2: Due in 6 hours (MEDIUM priority)
   - Task 3: Due in 23 hours (LOW priority)

## Step 6: Monitor Worker Logs

Watch the worker console for output like:

```
[14:30:10 INF] Starting reminder check cycle
[14:30:10 INF] Email quota status: 0/90 sent today, max 50 this run
[14:30:11 INF] Found 3 tasks needing reminders
[14:30:12 INF] Email sent successfully to user@example.com for task: Complete Phase 4
[14:30:12 INF] Reminder sent for task 42: Complete Phase 4
```

## Step 7: Check Your Email

Within a few minutes, you should receive beautiful reminder emails for each task.

## Step 8: Verify Idempotency

1. Wait for the next worker cycle (30 minutes in dev)
2. Check logs - should say "Found 0 tasks needing reminders"
3. Verify NO duplicate emails are received
4. Check audit log in database:

```sql
SELECT * FROM "AuditLogs" 
WHERE "Action" LIKE '%Reminder sent%' 
ORDER BY "Timestamp" DESC;
```

## Testing Different Scenarios

### Scenario 1: Urgent Task (Due in < 2 hours)

```
Title: Fix Critical Bug
Due Date: [2 hours from now]
Priority: Critical
```

Expected: Email with "⚠️ URGENT: Due in less than 2 hours!" message

### Scenario 2: Upcoming Task (Due in 6 hours)

```
Title: Review Pull Request
Due Date: [6 hours from now]
Priority: Medium
```

Expected: Email with "⏰ Due very soon!" message

### Scenario 3: Task Due Tomorrow

```
Title: Weekly Planning
Due Date: [23 hours from now]
Priority: Low
```

Expected: Email with "📅 Upcoming task reminder" message

### Scenario 4: No Duplicates

1. Create task with due date in 2 hours
2. Wait for first reminder (check email)
3. Wait for second worker cycle (30 minutes)
4. Verify NO second email is sent

### Scenario 5: Quota Limit

1. Set `DailyEmailQuota` to 3 in appsettings.json
2. Create 5 tasks due in next 24 hours
3. Run worker
4. Verify only 3 emails are sent
5. Check logs for quota warning

### Scenario 6: Dry Run Mode

1. Set `EnableEmailSending` to `false`
2. Run worker
3. Check logs for "Email sending disabled (dry run)" messages
4. Verify no actual emails are sent
5. Verify audit logs still created

## Docker Testing

### Test Worker in Docker

```powershell
# Build and run postgres + worker
docker-compose up -d postgres
docker-compose up --build worker

# View worker logs in real-time
docker-compose logs -f worker

# Stop worker
docker-compose down worker
```

### Test Full Stack (API + UI + Worker + Postgres)

```powershell
# Uncomment API and UI services in docker-compose.yml first
docker-compose up --build

# Access UI at http://localhost:3000
```

## Troubleshooting

### Problem: Worker can't connect to database

**Solution 1**: Check PostgreSQL is running
```powershell
docker ps | Select-String "postgres"
```

**Solution 2**: Verify connection string in appsettings.json
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=tasktracker;Username=postgres;Password=postgres"
}
```

**Solution 3**: Check database name matches
```powershell
docker exec -it tasktracker-postgres psql -U postgres -l
```

### Problem: No emails being sent

**Solution 1**: Check Mailgun API key is correct
- Verify in appsettings.json
- Test at https://app.mailgun.com/

**Solution 2**: Add recipient to authorized list
- Go to Mailgun dashboard
- Domains > [your domain] > Authorized Recipients
- Add email and verify

**Solution 3**: Check EnableEmailSending setting
```json
"WorkerSettings": {
  "EnableEmailSending": true
}
```

**Solution 4**: Check daily quota not exceeded
- Look for "Daily email quota reached" in logs
- Check `SELECT COUNT(*) FROM "AuditLogs" WHERE "Action" LIKE '%Reminder sent%' AND "Timestamp" >= CURRENT_DATE;`

### Problem: Duplicate emails being sent

**Solution 1**: Check audit log table exists
```powershell
docker exec -it tasktracker-postgres psql -U postgres -d tasktracker -c "\dt"
```

**Solution 2**: Verify audit entries are being created
```sql
SELECT * FROM "AuditLogs" WHERE "Action" LIKE '%Reminder sent%';
```

**Solution 3**: Check database transactions are committing
- Look for errors in worker logs
- Verify database connection is stable

### Problem: Worker crashes on startup

**Solution 1**: Check .NET 9.0 SDK is installed
```powershell
dotnet --version
```

**Solution 2**: Check all dependencies are restored
```powershell
dotnet restore
dotnet build
```

**Solution 3**: Check appsettings.json is valid JSON
- Use JSON validator
- Check for missing commas, quotes, etc.

## Performance Tips

### For Development

- Set `CheckIntervalMinutes` to 5 for faster testing
- Set `DailyEmailQuota` to 10 to test quota logic
- Set `EnableEmailSending` to `false` for testing without emails

### For Production

- Set `CheckIntervalMinutes` to 60 (or higher)
- Set `DailyEmailQuota` to 90 (leave 10 buffer)
- Enable `restart: unless-stopped` in docker-compose.yml
- Monitor logs with aggregation tool (ELK, Seq, etc.)
- Set up health check monitoring

## Verification Checklist

After running the worker, verify:

- [ ] Worker starts without errors
- [ ] Connects to PostgreSQL successfully
- [ ] Logs show "Database connection successful"
- [ ] First reminder cycle runs after 10 seconds
- [ ] Tasks are found (if any due within 24 hours)
- [ ] Emails are sent successfully
- [ ] Audit logs are created with "Reminder sent" action
- [ ] Second cycle does NOT send duplicates
- [ ] Daily quota is respected
- [ ] Worker shuts down cleanly with Ctrl+C

## Next Steps

1. Test locally with sample tasks ✓
2. Verify idempotency works ✓
3. Test email quota limits ✓
4. Test Docker deployment ✓
5. Add to CI/CD pipeline
6. Set up monitoring and alerting
7. Create unit tests for worker logic
8. Document operational runbook

## Support

If you encounter issues:

1. Check logs in `TaskTracker.Worker/logs/` folder
2. Enable debug logging in appsettings.json
3. Verify Mailgun dashboard for API errors
4. Check PostgreSQL logs: `docker logs tasktracker-postgres`
5. Review worker README.md for detailed documentation
