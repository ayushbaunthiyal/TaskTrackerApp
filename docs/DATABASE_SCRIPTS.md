# 🗄️ Database Management Scripts

Complete collection of SQL scripts for managing the TaskTracker PostgreSQL database.

## 📋 Table of Contents

1. [Quick Reference](#quick-reference)
2. [User Management](#user-management)
3. [Data Management](#data-management)
4. [Verification Scripts](#verification-scripts)
5. [Usage Instructions](#usage-instructions)

---

## Quick Reference

| Script | Purpose | Use Case |
|--------|---------|----------|
| Check Users | View all users | See who exists in database |
| Clear All Data | Delete everything | Fresh start with clean database |
| Clear and Reseed | Delete tasks/attachments only | Keep users, reseed sample data |
| Manual Task Seeding | Insert sample tasks | Add test data manually |
| Verify Audit Logs | Check audit trail | Ensure logging is working |

---

## User Management

### Check All Users

View all users in the database with their creation dates and status.

```sql
-- Check all users in the database
SELECT "Id", "Email", "CreatedAt", "IsDeleted" 
FROM "Users" 
ORDER BY "Email";
```

**When to use:**
- Before manual task seeding (to get User IDs)
- To verify user accounts exist
- To check for deleted users

---

## Data Management

### 1. Clear All Data (Complete Reset)

**⚠️ WARNING: This deletes ALL data including users!**

```sql
-- Delete all data from all tables
DELETE FROM "Attachments";
DELETE FROM "Tasks";
DELETE FROM "AuditLogs";
DELETE FROM "Users";
```

**When to use:**
- Complete fresh start
- Before running migrations from scratch
- Testing initial seeding logic

**After running:**
- Restart the API to trigger automatic seeding
- Or manually insert users and tasks

---

### 2. Clear and Reseed (Keep Users)

Delete only tasks and attachments, preserving users. Allows automatic reseeding.

```sql
-- Clear existing data to allow reseeding
DELETE FROM "Attachments";
DELETE FROM "AuditLogs" WHERE "EntityType" = 'TaskItem' OR "EntityType" = 'Attachment';
DELETE FROM "Tasks";

-- The application will automatically reseed data on next startup
```

**When to use:**
- Want fresh tasks but keep existing users
- Testing task seeding logic
- Resetting demo data

**After running:**
- Restart the API
- Application will auto-seed 10 sample tasks

---

### 3. Clear Data with Verification

Complete data clear with verification step to confirm deletion.

```sql
-- Clear existing data to allow re-seeding with audit logs
-- Run this script in DBeaver/pgAdmin before restarting the API

-- Delete all existing data (in proper order due to foreign keys)
DELETE FROM "Attachments";
DELETE FROM "Tasks";
DELETE FROM "AuditLogs";
DELETE FROM "Users";

-- Verify tables are empty
SELECT 'Users' as "Table", COUNT(*) as "Count" FROM "Users"
UNION ALL
SELECT 'Tasks', COUNT(*) FROM "Tasks"
UNION ALL
SELECT 'Attachments', COUNT(*) FROM "Attachments"
UNION ALL
SELECT 'AuditLogs', COUNT(*) FROM "AuditLogs";

-- After running this, restart the API and it will re-seed with audit logs
```

**When to use:**
- Want confirmation that data is cleared
- Before important seeding operations
- Debugging seeding issues

---

### 4. Manual Task Seeding

Insert 10 sample tasks manually when automatic seeding isn't working.

**Step 1: Get User IDs**

```sql
-- First, get the User IDs (copy these values for use below)
SELECT "Id", "Email" FROM "Users" ORDER BY "Email";
```

**Step 2: Insert Tasks (Replace USER GUIDs)**

```sql
-- Manual Task Seeding Script for TaskTracker Database
-- Replace 'USER1_GUID_HERE', 'USER2_GUID_HERE', 'USER3_GUID_HERE' with actual GUIDs

-- Insert 10 sample tasks
INSERT INTO "Tasks" ("Id", "UserId", "Title", "Description", "Status", "Priority", "DueDate", "Tags", "CreatedAt", "UpdatedAt", "IsDeleted")
VALUES
-- Task 1 - Complete project documentation (InProgress, High Priority)
(
    gen_random_uuid(),
    'USER1_GUID_HERE',  -- john.doe@example.com
    'Complete project documentation',
    'Write comprehensive documentation for the Task Tracker API',
    2,  -- InProgress
    3,  -- High
    NOW() + INTERVAL '7 days',
    '["documentation", "api", "priority"]'::jsonb,
    NOW() - INTERVAL '5 days',
    NOW() - INTERVAL '2 days',
    false
),

-- Task 2 - Fix authentication bug (Pending, Critical)
(
    gen_random_uuid(),
    'USER1_GUID_HERE',  -- john.doe@example.com
    'Fix authentication bug',
    'Resolve the issue with JWT token expiration',
    1,  -- Pending
    4,  -- Critical
    NOW() + INTERVAL '2 days',
    '["bug", "authentication", "urgent"]'::jsonb,
    NOW() - INTERVAL '3 days',
    NOW() - INTERVAL '3 days',
    false
),

-- Task 3 - Design new landing page (Completed, Medium)
(
    gen_random_uuid(),
    'USER2_GUID_HERE',  -- jane.smith@example.com
    'Design new landing page',
    'Create mockups for the new landing page design',
    3,  -- Completed
    2,  -- Medium
    NOW() - INTERVAL '1 day',
    '["design", "ui", "frontend"]'::jsonb,
    NOW() - INTERVAL '10 days',
    NOW() - INTERVAL '1 day',
    false
),

-- Task 4 - Update database schema (InProgress, Medium)
(
    gen_random_uuid(),
    'USER2_GUID_HERE',  -- jane.smith@example.com
    'Update database schema',
    'Add new columns for user preferences',
    2,  -- InProgress
    2,  -- Medium
    NOW() + INTERVAL '5 days',
    '["database", "schema", "backend"]'::jsonb,
    NOW() - INTERVAL '4 days',
    NOW() - INTERVAL '1 day',
    false
),

-- Task 5 - Setup CI/CD pipeline (Pending, High)
(
    gen_random_uuid(),
    'USER3_GUID_HERE',  -- bob.wilson@example.com
    'Setup CI/CD pipeline',
    'Configure automated deployment pipeline',
    1,  -- Pending
    3,  -- High
    NOW() + INTERVAL '10 days',
    '["devops", "ci-cd", "automation"]'::jsonb,
    NOW() - INTERVAL '2 days',
    NOW() - INTERVAL '2 days',
    false
),

-- Task 6 - Code review for PR #123 (Completed, Low)
(
    gen_random_uuid(),
    'USER3_GUID_HERE',  -- bob.wilson@example.com
    'Code review for PR #123',
    'Review the pull request for the new feature',
    3,  -- Completed
    1,  -- Low
    NOW() - INTERVAL '2 days',
    '["review", "code-quality"]'::jsonb,
    NOW() - INTERVAL '6 days',
    NOW() - INTERVAL '3 days',
    false
),

-- Task 7 - Implement rate limiting (Pending, Medium)
(
    gen_random_uuid(),
    'USER1_GUID_HERE',  -- john.doe@example.com
    'Implement rate limiting',
    'Add rate limiting middleware to the API',
    1,  -- Pending
    2,  -- Medium
    NOW() + INTERVAL '14 days',
    '["security", "api", "performance"]'::jsonb,
    NOW() - INTERVAL '1 day',
    NOW() - INTERVAL '1 day',
    false
),

-- Task 8 - Performance optimization (InProgress, High)
(
    gen_random_uuid(),
    'USER2_GUID_HERE',  -- jane.smith@example.com
    'Performance optimization',
    'Optimize database queries for better performance',
    2,  -- InProgress
    3,  -- High
    NOW() + INTERVAL '8 days',
    '["performance", "optimization", "database"]'::jsonb,
    NOW() - INTERVAL '7 days',
    NOW(),
    false
),

-- Task 9 - Write unit tests (Pending, Medium)
(
    gen_random_uuid(),
    'USER3_GUID_HERE',  -- bob.wilson@example.com
    'Write unit tests',
    'Increase test coverage to 80%',
    1,  -- Pending
    2,  -- Medium
    NOW() + INTERVAL '12 days',
    '["testing", "quality", "coverage"]'::jsonb,
    NOW() - INTERVAL '1 day',
    NOW() - INTERVAL '1 day',
    false
),

-- Task 10 - Update dependencies (Cancelled, Low)
(
    gen_random_uuid(),
    'USER1_GUID_HERE',  -- john.doe@example.com
    'Update dependencies',
    'Update all NuGet packages to latest versions',
    4,  -- Cancelled
    1,  -- Low
    NOW() + INTERVAL '20 days',
    '["maintenance", "dependencies"]'::jsonb,
    NOW() - INTERVAL '8 days',
    NOW() - INTERVAL '4 days',
    false
);

-- Verify tasks were created
SELECT COUNT(*) as "TaskCount" FROM "Tasks";
SELECT "Title", "Status", "Priority" FROM "Tasks" ORDER BY "CreatedAt";
```

**Status Values:**
- 1 = Pending
- 2 = InProgress
- 3 = Completed
- 4 = Cancelled

**Priority Values:**
- 1 = Low
- 2 = Medium
- 3 = High
- 4 = Critical

**When to use:**
- Automatic seeding didn't work
- Need specific test data
- Creating demo environment

---

## Verification Scripts

### Verify Audit Logs

Check that audit logging is working correctly.

```sql
-- Verify audit log entries
SELECT COUNT(*) as total_audit_logs FROM "AuditLogs";

-- Count by entity type
SELECT "EntityType", COUNT(*) as count 
FROM "AuditLogs" 
GROUP BY "EntityType" 
ORDER BY "EntityType";

-- View all audit log entries
SELECT "Action", "EntityType", "EntityId", "Timestamp", "Details"
FROM "AuditLogs"
ORDER BY "Timestamp";
```

**What to look for:**
- **Total count**: Should match number of create/update/delete operations
- **Entity types**: TaskItem, Attachment, User
- **Actions**: Created, Updated, Deleted, ReminderSent
- **Recent entries**: Check timestamps are current

---

## Usage Instructions

### Accessing PostgreSQL

**Option 1: Docker Container**
```powershell
docker exec -it tasktracker-postgres psql -U tasktracker_user -d TaskTrackerDB
```

**Option 2: GUI Tool (DBeaver/pgAdmin)**
- Host: localhost
- Port: 5433
- Database: TaskTrackerDB
- Username: tasktracker_user
- Password: TaskTracker123!

### Running Scripts

**Method 1: Copy and paste**
1. Open database connection
2. Copy script from this document
3. Paste into SQL editor
4. Execute

**Method 2: From file**
```powershell
# If you saved individual .sql files
docker exec -i tasktracker-postgres psql -U tasktracker_user -d TaskTrackerDB < script.sql
```

### Common Workflows

**Workflow 1: Fresh Start with Auto-Seeding**
1. Run "Clear All Data" script
2. Restart API (`docker-compose restart api`)
3. Application seeds 3 users + 10 tasks automatically

**Workflow 2: Keep Users, Refresh Tasks**
1. Run "Clear and Reseed" script
2. Restart API
3. Users remain, tasks are reseeded

**Workflow 3: Manual Task Creation**
1. Run "Check Users" to get User IDs
2. Copy GUIDs
3. Run "Manual Task Seeding" with replaced GUIDs
4. Run verification queries

**Workflow 4: Verify Everything is Working**
1. Run "Verify Audit Logs" script
2. Check counts match expected operations
3. Verify recent timestamps

---

## Troubleshooting

### No tasks showing up after seeding?

```sql
-- Check if tasks exist but are soft-deleted
SELECT COUNT(*) FROM "Tasks" WHERE "IsDeleted" = false;
SELECT COUNT(*) FROM "Tasks" WHERE "IsDeleted" = true;
```

### Foreign key constraint errors?

Delete in this order:
1. Attachments (references Tasks)
2. Tasks (references Users)
3. AuditLogs (references Users and Tasks)
4. Users (last)

### Can't see seeded data?

```sql
-- Check all tables
SELECT 'Users' as "Table", COUNT(*) FROM "Users"
UNION ALL SELECT 'Tasks', COUNT(*) FROM "Tasks"
UNION ALL SELECT 'Attachments', COUNT(*) FROM "Attachments"
UNION ALL SELECT 'AuditLogs', COUNT(*) FROM "AuditLogs";
```

### Need to reset everything?

```powershell
# Nuclear option: destroy and recreate database
docker-compose down -v
docker-compose up -d
# Wait for migrations and seeding to complete
```

---

## Best Practices

1. **Always verify before deleting**: Run SELECT queries first
2. **Backup before major changes**: Use `pg_dump` for backups
3. **Use transactions for manual changes**: Wrap in BEGIN/COMMIT/ROLLBACK
4. **Check audit logs**: Verify operations are logged correctly
5. **Document manual changes**: Keep track of what you've done

---

## Additional Resources

- **Docker Deployment**: See `docs/DOCKER_DEPLOYMENT.md`
- **Phase 5 Documentation**: See `docs/Phases/Phase5_Monitoring_Metrics_HealthChecks_Docker.md`
- **API Documentation**: http://localhost:5128/swagger
- **PostgreSQL Docs**: https://www.postgresql.org/docs/

---

**Database Connection Info:**
- **Host**: localhost
- **Port**: 5433 (host) → 5432 (container)
- **Database**: TaskTrackerDB
- **Username**: tasktracker_user
- **Password**: TaskTracker123! (⚠️ change in production!)
