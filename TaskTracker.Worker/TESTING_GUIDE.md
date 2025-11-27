# 🚀 Complete Testing Guide - TaskTracker Worker Service

This guide will walk you through testing the complete TaskTracker application with the new background worker service.

## 📋 Prerequisites Checklist

- [x] .NET 9 SDK installed
- [x] Docker Desktop running
- [x] PostgreSQL container running
- [x] Mailgun account with API key
- [x] Email added to Mailgun authorized recipients

## 🎯 Testing Scenarios

### Scenario 1: First Time Setup (Fresh Install)

**Time Required**: 15 minutes

```powershell
# 1. Navigate to project root
cd d:\repos\mygit\TaskTrackerApp

# 2. Start PostgreSQL
docker-compose up -d postgres
Start-Sleep -Seconds 10

# 3. Run test script
cd TaskTracker.Worker
.\test-worker.ps1

# 4. Run worker
dotnet run
```

**Expected Output**:
```
[14:30:00 INF] Reminder Hosted Service started at 2025-11-27 14:30:00
[14:30:00 INF] Check interval: 30 minutes
[14:30:00 INF] Lookahead window: 24 hours
[14:30:10 INF] Database connection successful
[14:30:10 INF] Starting reminder check cycle at 2025-11-27 14:30:10
```

### Scenario 2: Test Email Sending

**Time Required**: 5 minutes (or 30 minutes if waiting for scheduled cycle)

**Option A: Quick Test (1-minute interval)**

1. Edit `appsettings.json`:
```json
{
  "WorkerSettings": {
    "CheckIntervalMinutes": 1
  }
}
```

2. Create test task via UI:
   - Login at http://localhost:3000
   - Create task with due date in 2 hours
   - Priority: Critical

3. Wait 1 minute for worker cycle

4. Check your email inbox

**Option B: Normal Test (30-minute interval)**

1. Keep default `CheckIntervalMinutes: 30`
2. Create multiple test tasks
3. Wait for scheduled cycle
4. Check email inbox

**Expected Email**:
- Subject: "⏰ Task Reminder: [Your Task Title]"
- Beautiful HTML email with gradient header
- Task details with priority badge
- Time remaining calculation
- Link to TaskTracker UI

### Scenario 3: Test Idempotency (No Duplicates)

**Time Required**: 2-3 minutes (after first reminder sent)

```powershell
# After receiving first reminder, wait for next cycle
# Worker should log: "Found 0 tasks needing reminders"
```

**Verification Steps**:

1. Check worker logs for second cycle:
```
[15:00:10 INF] Found 0 tasks needing reminders
```

2. Check database audit log:
```sql
SELECT * FROM "AuditLogs" 
WHERE "Action" LIKE '%Reminder sent%' 
ORDER BY "Timestamp" DESC;
```

3. Verify email inbox - NO duplicate emails received

**Success Criteria**:
✅ Second cycle finds 0 tasks  
✅ No duplicate emails received  
✅ Audit log shows only ONE entry per task  

### Scenario 4: Test Email Quota Limits

**Time Required**: 5 minutes

```json
{
  "WorkerSettings": {
    "CheckIntervalMinutes": 1,
    "DailyEmailQuota": 3
  }
}
```

1. Create 5 tasks with due dates in next 24 hours
2. Run worker
3. Check logs for quota warning

**Expected Output**:
```
[14:30:11 INF] Found 5 tasks needing reminders
[14:30:12 INF] Email sent successfully to user1@example.com
[14:30:13 INF] Email sent successfully to user2@example.com
[14:30:14 INF] Email sent successfully to user3@example.com
[14:30:14 INF] Reminder processing completed. Sent: 3, Failed: 0, Total today: 3
```

Next cycle:
```
[14:31:10 WRN] Daily email quota reached (3/3). Skipping reminder processing.
```

**Success Criteria**:
✅ Only 3 emails sent  
✅ 2 tasks skipped  
✅ Warning logged  
✅ Next cycle skips processing  

### Scenario 5: Test Priority-Based Sorting

**Time Required**: 5 minutes

Create 4 tasks with same due date but different priorities:

| Task | Due Date | Priority | Expected Order |
|------|----------|----------|----------------|
| Task D | 2 hours | Low | 4th |
| Task C | 2 hours | Medium | 3rd |
| Task B | 2 hours | High | 2nd |
| Task A | 2 hours | Critical | 1st |

Check worker logs to see processing order:

```
[14:30:12 INF] Reminder sent for task 101: Task A (Critical)
[14:30:13 INF] Reminder sent for task 102: Task B (High)
[14:30:14 INF] Reminder sent for task 103: Task C (Medium)
[14:30:15 INF] Reminder sent for task 104: Task D (Low)
```

**Success Criteria**:
✅ Critical priority processed first  
✅ Low priority processed last  
✅ All emails sent in correct order  

### Scenario 6: Test Dry Run Mode (No Actual Emails)

**Time Required**: 2 minutes

```json
{
  "WorkerSettings": {
    "CheckIntervalMinutes": 1,
    "EnableEmailSending": false
  }
}
```

**Expected Output**:
```
[14:30:12 INF] Email sending disabled. Would send reminder for task 42: Complete Phase 4
```

**Verification**:
1. Audit log still created: ✅
2. No actual emails sent: ✅
3. Log shows "Email sending disabled (dry run)": ✅

### Scenario 7: Test Docker Deployment

**Time Required**: 10 minutes

```powershell
# Build and run full stack
cd d:\repos\mygit\TaskTrackerApp
docker-compose up --build

# In separate terminal, view worker logs
docker-compose logs -f worker

# Verify worker is running
docker ps
```

**Expected Output**:
```
CONTAINER ID   IMAGE                    STATUS
abc123def456   tasktracker-worker       Up 2 minutes
789ghi012jkl   tasktracker-postgres     Up 3 minutes (healthy)
```

**Worker Logs**:
```
tasktracker-worker | [14:30:00 INF] Reminder Hosted Service started
tasktracker-worker | [14:30:10 INF] Database connection successful
```

**Success Criteria**:
✅ Worker container starts  
✅ Connects to postgres container  
✅ Runs reminder cycles  
✅ Sends emails successfully  

### Scenario 8: Test Error Handling

**Time Required**: 5 minutes

**Test 1: Invalid Mailgun API Key**

1. Change API key to `invalid-key-12345`
2. Run worker
3. Create task

**Expected**:
```
[14:30:12 ERR] Failed to send email to user@example.com. Status: Unauthorized
```

**Test 2: Database Connection Lost**

1. Stop PostgreSQL: `docker-compose stop postgres`
2. Worker should log error
3. Restart PostgreSQL: `docker-compose start postgres`
4. Worker should reconnect on next cycle

**Expected**:
```
[14:30:12 ERR] Error in reminder processing
[14:31:12 INF] Starting reminder check cycle
[14:31:12 INF] Database connection successful
```

**Success Criteria**:
✅ Errors logged clearly  
✅ Worker doesn't crash  
✅ Recovers automatically  

## 🔍 Verification Checklist

After running all scenarios, verify:

### Worker Service
- [ ] Starts without errors
- [ ] Connects to PostgreSQL
- [ ] Runs scheduled cycles
- [ ] Logs comprehensively
- [ ] Shuts down gracefully (Ctrl+C)

### Email Delivery
- [ ] Sends beautiful HTML emails
- [ ] Includes all task details
- [ ] Shows correct priority badges
- [ ] Calculates time remaining
- [ ] Links to TaskTracker UI

### Idempotency
- [ ] Each task gets ONE reminder
- [ ] Audit log prevents duplicates
- [ ] Second cycle finds 0 tasks
- [ ] No duplicate emails received

### Quota Management
- [ ] Respects daily limit
- [ ] Logs quota status
- [ ] Stops at threshold
- [ ] Resets at midnight UTC

### Error Handling
- [ ] Handles invalid API key
- [ ] Recovers from database errors
- [ ] Logs errors clearly
- [ ] Doesn't crash on failures

### Docker Deployment
- [ ] Builds successfully
- [ ] Runs in container
- [ ] Connects to postgres container
- [ ] Logs visible via docker-compose
- [ ] Restarts automatically

## 📊 Database Verification Queries

### Check Reminders Sent Today

```sql
SELECT 
    COUNT(*) as TotalReminders,
    COUNT(DISTINCT "TaskId") as UniqueTasks
FROM "AuditLogs"
WHERE "Action" LIKE '%Reminder sent%'
AND "Timestamp" >= CURRENT_DATE;
```

### List All Reminders with Task Details

```sql
SELECT 
    al."Timestamp",
    t."Title",
    t."DueDate",
    t."Priority",
    u."Email",
    al."Action"
FROM "AuditLogs" al
JOIN "Tasks" t ON al."TaskId" = t."Id"
JOIN "Users" u ON al."UserId" = u."Id"
WHERE al."Action" LIKE '%Reminder sent%'
ORDER BY al."Timestamp" DESC;
```

### Find Tasks Eligible for Reminders

```sql
SELECT 
    "Id",
    "Title",
    "DueDate",
    "Priority",
    "Status"
FROM "Tasks"
WHERE "DueDate" BETWEEN NOW() AND NOW() + INTERVAL '24 hours'
AND "Status" != 3  -- Not Completed
AND "IsDeleted" = false;
```

### Check for Duplicate Reminders (Should be 0)

```sql
SELECT 
    "TaskId",
    COUNT(*) as ReminderCount
FROM "AuditLogs"
WHERE "Action" LIKE '%Reminder sent%'
GROUP BY "TaskId"
HAVING COUNT(*) > 1;
```

## 🐛 Troubleshooting Guide

### Problem: Worker can't connect to database

**Check 1**: PostgreSQL running?
```powershell
docker ps | Select-String "postgres"
```

**Check 2**: Connection string correct?
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=tasktracker;Username=postgres;Password=postgres"
}
```

**Check 3**: Database exists?
```powershell
docker exec -it tasktracker-postgres psql -U postgres -l
```

### Problem: No emails being sent

**Check 1**: Mailgun API key valid?
- Test at https://app.mailgun.com/

**Check 2**: Recipient authorized?
- Mailgun dashboard > Authorized Recipients

**Check 3**: EmailSending enabled?
```json
"WorkerSettings": {
  "EnableEmailSending": true
}
```

**Check 4**: Quota not exceeded?
```sql
SELECT COUNT(*) FROM "AuditLogs" 
WHERE "Action" LIKE '%Reminder sent%' 
AND "Timestamp" >= CURRENT_DATE;
```

### Problem: Duplicate emails

**Check 1**: Audit table exists?
```powershell
docker exec -it tasktracker-postgres psql -U postgres -d tasktracker -c "\dt"
```

**Check 2**: Audit entries created?
```sql
SELECT * FROM "AuditLogs" WHERE "Action" LIKE '%Reminder sent%';
```

**Check 3**: Database transactions committing?
- Look for errors in worker logs

### Problem: Worker crashes

**Check 1**: .NET 9 installed?
```powershell
dotnet --version
```

**Check 2**: Dependencies restored?
```powershell
dotnet restore
dotnet build
```

**Check 3**: appsettings.json valid?
- Use online JSON validator

## 📈 Performance Monitoring

### Monitor Worker Resource Usage

```powershell
# Docker stats
docker stats tasktracker-worker

# Expected:
# CPU: < 1%
# Memory: 50-100 MB
```

### Monitor Log File Size

```powershell
# Check log size
Get-ChildItem TaskTracker.Worker\logs\*.log | Measure-Object -Property Length -Sum
```

### Monitor Database Queries

```sql
-- Enable query logging in PostgreSQL
ALTER SYSTEM SET log_statement = 'all';
SELECT pg_reload_conf();

-- View logs
docker logs tasktracker-postgres
```

## ✅ Final Acceptance Test

Run this complete test to verify everything works:

```powershell
# 1. Start services
docker-compose up -d postgres
Start-Sleep -Seconds 10

# 2. Run worker
cd TaskTracker.Worker
dotnet run

# In separate terminal:

# 3. Start API
cd TaskTracker.API
dotnet run

# 4. Start UI
cd task-tracker-ui
npm start

# 5. Open browser
# http://localhost:3000

# 6. Create test account
# Email: test@example.com
# Password: Test123!

# 7. Add email to Mailgun authorized recipients
# https://app.mailgun.com/

# 8. Create 3 test tasks
# - Task 1: Due in 2 hours, Critical
# - Task 2: Due in 6 hours, High
# - Task 3: Due in 23 hours, Medium

# 9. Wait for worker cycle (30 min or 1 min if changed)

# 10. Verify:
# ✅ 3 emails received
# ✅ Beautiful HTML formatting
# ✅ Correct task details
# ✅ Priority color-coding
# ✅ Links work

# 11. Wait for second cycle

# 12. Verify:
# ✅ No duplicate emails
# ✅ Worker logs "Found 0 tasks"
# ✅ Audit log shows 3 entries only

# 13. Shutdown
# Ctrl+C in all terminals
# docker-compose down
```

**If all checks pass**: ✅ **WORKER SERVICE IS PRODUCTION READY!**

## 🚀 Next Steps

1. **Merge to Master**
```powershell
git add .
git commit -m "feat: Add background worker service with email notifications"
git checkout master
git merge phase3_reactUI
git push origin master
```

2. **Create GitHub Release**
- Tag: v1.0.0
- Title: "TaskTracker v1.0 - Full Stack with Background Worker"
- Include Phase4_Worker_Implementation.md in release notes

3. **Deploy to Cloud**
- Azure Container Apps
- AWS ECS
- DigitalOcean App Platform
- Heroku

4. **Set Up Monitoring**
- Application Insights
- Datadog
- New Relic
- ELK Stack

5. **Write Tests**
- Unit tests for ReminderService
- Integration tests for worker
- End-to-end tests

## 📞 Support

If you encounter issues:

1. Check logs: `TaskTracker.Worker/logs/`
2. Review README: `TaskTracker.Worker/README.md`
3. Check Mailgun dashboard for API errors
4. Verify database with queries above
5. Review worker logs: `docker-compose logs worker`

---

**Congratulations!** 🎉 You've successfully implemented and tested the TaskTracker background worker service!
