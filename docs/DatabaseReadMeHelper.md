# PostgreSQL Database Setup & Query Guide

This guide helps you connect to and query the TaskTracker PostgreSQL database using GUI tools.

## 🎯 Recommended GUI Tools for PostgreSQL

### 1. **pgAdmin 4** (Most Popular - Official PostgreSQL Tool)
- ✅ **Official PostgreSQL GUI** - Industry standard
- ✅ **Free & Open Source**
- ✅ **Cross-platform** (Windows, macOS, Linux)
- ✅ **Feature-rich** - Query tool, visual explain, backup/restore, monitoring
- ✅ **Web-based interface** (runs in browser)

**Download:** https://www.pgadmin.org/download/

**Quick Setup:**
1. Download and install pgAdmin 4
2. Open pgAdmin (it runs in your browser)
3. Right-click "Servers" → Create → Server
4. **Connection Details:**
   - Name: `TaskTracker Local`
   - Host: `localhost`
   - Port: `5433` (from our docker-compose)
   - Database: `TaskTrackerDB`
   - Username: `tasktracker_user`
   - Password: `TaskTracker123!`

---

### 2. **DBeaver Community Edition** (Recommended)
- ✅ **Universal database tool** (supports PostgreSQL, MySQL, SQL Server, etc.)
- ✅ **Free & Open Source**
- ✅ **Clean, modern UI**
- ✅ **Great SQL editor** with auto-complete
- ✅ **ER diagrams** - Visualize table relationships
- ✅ **Data export** in multiple formats (CSV, JSON, Excel)

**Download:** https://dbeaver.io/download/

**Quick Setup:**
1. Download and install DBeaver Community
2. Click "New Database Connection" (plug icon)
3. Select "PostgreSQL"
4. **Connection Details:**
   - Host: `localhost`
   - Port: `5433`
   - Database: `TaskTrackerDB`
   - Username: `tasktracker_user`
   - Password: `TaskTracker123!`
5. Click "Test Connection" → "Finish"

---

### 3. **Visual Studio Code with PostgreSQL Extension**
- ✅ **Lightweight** - Works in your existing editor
- ✅ **Free**
- ✅ **Quick queries** without switching apps

**Setup:**
1. Install extension: "PostgreSQL" by Chris Kolkman
2. Press `Ctrl+Shift+P` → Type "PostgreSQL: New Query"
3. Enter connection string:
   ```
   postgresql://tasktracker_user:TaskTracker123!@localhost:5433/TaskTrackerDB
   ```

---

## 📊 Connection Information

**Database Configuration:**
```
Host:     localhost
Port:     5433
Database: TaskTrackerDB
Username: tasktracker_user
Password: TaskTracker123!
```

**Docker Container:**
```bash
# Start PostgreSQL container
docker-compose up -d

# Stop PostgreSQL container
docker-compose down

# View container logs
docker logs tasktracker-postgres

# Check if container is running
docker ps
```

---

## 🔍 Useful SQL Queries

### **1. View All Tables**
```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public';
```

Expected tables:
- `Users`
- `Tasks`
- `Attachments`
- `AuditLogs`
- `__EFMigrationsHistory`

---

### **2. See All Tasks**
```sql
SELECT 
    "Id",
    "Title",
    "Status",
    "Priority",
    "DueDate",
    "Tags",
    "CreatedAt"
FROM "Tasks"
WHERE "IsDeleted" = false
ORDER BY "CreatedAt" DESC;
```

**Status Values:**
- `1` = Pending
- `2` = InProgress
- `3` = Completed
- `4` = Cancelled

**Priority Values:**
- `1` = Low
- `2` = Medium
- `3` = High
- `4` = Critical

---

### **3. View Tasks with User Information**
```sql
SELECT 
    t."Title",
    t."Status",
    t."Priority",
    t."DueDate",
    u."Email" as "UserEmail",
    t."Tags"
FROM "Tasks" t
INNER JOIN "Users" u ON t."UserId" = u."Id"
WHERE t."IsDeleted" = false
ORDER BY t."CreatedAt" DESC;
```

---

### **4. See Audit Trail for a Specific Task**
```sql
SELECT 
    "Action",
    "Timestamp",
    "Details",
    u."Email" as "PerformedBy"
FROM "AuditLogs" al
LEFT JOIN "Users" u ON al."UserId" = u."Id"
WHERE al."EntityType" = 'TaskItem' 
  AND al."EntityId" = 'paste-task-guid-here'
ORDER BY "Timestamp" DESC;
```

---

### **5. Count Tasks by Status**
```sql
SELECT 
    CASE "Status"
        WHEN 1 THEN 'Pending'
        WHEN 2 THEN 'InProgress'
        WHEN 3 THEN 'Completed'
        WHEN 4 THEN 'Cancelled'
    END as "StatusName",
    COUNT(*) as "Count"
FROM "Tasks"
WHERE "IsDeleted" = false
GROUP BY "Status"
ORDER BY "Status";
```

---

### **6. Find Tasks Due Soon (Next 7 Days)**
```sql
SELECT 
    "Title",
    "DueDate",
    "Priority",
    "Status"
FROM "Tasks"
WHERE "IsDeleted" = false
  AND "DueDate" BETWEEN NOW() AND NOW() + INTERVAL '7 days'
ORDER BY "DueDate" ASC;
```

---

### **7. Search Tasks by Tag (PostgreSQL JSONB Query)**
```sql
-- Find all tasks with "work" tag
SELECT 
    "Title",
    "Tags",
    "Status"
FROM "Tasks"
WHERE "IsDeleted" = false
  AND "Tags" @> '["work"]'::jsonb
ORDER BY "CreatedAt" DESC;

-- Find tasks with any of multiple tags
SELECT 
    "Title",
    "Tags"
FROM "Tasks"
WHERE "IsDeleted" = false
  AND ("Tags" @> '["work"]'::jsonb OR "Tags" @> '["urgent"]'::jsonb);
```

**JSONB Operators:**
- `@>` - Contains (e.g., `Tags @> '["work"]'` means tags contain "work")
- `?` - Key exists (e.g., `Tags ? 'urgent'`)
- `?|` - Any key exists
- `?&` - All keys exist

---

### **8. View All Users with Task Counts**
```sql
SELECT 
    u."Id",
    u."Email",
    u."CreatedAt",
    COUNT(t."Id") as "TotalTasks",
    COUNT(CASE WHEN t."Status" = 1 THEN 1 END) as "PendingTasks",
    COUNT(CASE WHEN t."Status" = 2 THEN 1 END) as "InProgressTasks",
    COUNT(CASE WHEN t."Status" = 3 THEN 1 END) as "CompletedTasks"
FROM "Users" u
LEFT JOIN "Tasks" t ON u."Id" = t."UserId" AND t."IsDeleted" = false
WHERE u."IsDeleted" = false
GROUP BY u."Id", u."Email", u."CreatedAt"
ORDER BY "TotalTasks" DESC;
```

---

### **9. Recent Audit Activity (Last 24 Hours)**
```sql
SELECT 
    al."Action",
    al."EntityType",
    al."Timestamp",
    u."Email" as "User",
    al."Details"
FROM "AuditLogs" al
LEFT JOIN "Users" u ON al."UserId" = u."Id"
WHERE al."Timestamp" > NOW() - INTERVAL '24 hours'
ORDER BY al."Timestamp" DESC
LIMIT 50;
```

---

### **10. View Database Schema Details**
```sql
SELECT 
    c.table_name,
    c.column_name,
    c.data_type,
    c.is_nullable,
    c.column_default
FROM information_schema.columns c
WHERE c.table_schema = 'public'
  AND c.table_name NOT LIKE '__EF%'
ORDER BY c.table_name, c.ordinal_position;
```

---

### **11. View All Indexes**
```sql
SELECT 
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'public'
ORDER BY tablename, indexname;
```

---

### **12. Find Overdue Tasks**
```sql
SELECT 
    t."Title",
    t."DueDate",
    t."Priority",
    u."Email" as "AssignedTo",
    EXTRACT(DAY FROM (NOW() - t."DueDate")) as "DaysOverdue"
FROM "Tasks" t
INNER JOIN "Users" u ON t."UserId" = u."Id"
WHERE t."IsDeleted" = false
  AND t."Status" IN (1, 2)  -- Pending or InProgress
  AND t."DueDate" < NOW()
ORDER BY t."DueDate" ASC;
```

---

### **13. Get Task Statistics by Priority**
```sql
SELECT 
    CASE "Priority"
        WHEN 1 THEN 'Low'
        WHEN 2 THEN 'Medium'
        WHEN 3 THEN 'High'
        WHEN 4 THEN 'Critical'
    END as "PriorityName",
    COUNT(*) as "TotalTasks",
    COUNT(CASE WHEN "Status" = 3 THEN 1 END) as "CompletedTasks",
    ROUND(COUNT(CASE WHEN "Status" = 3 THEN 1 END)::numeric / COUNT(*)::numeric * 100, 2) as "CompletionRate"
FROM "Tasks"
WHERE "IsDeleted" = false
GROUP BY "Priority"
ORDER BY "Priority" DESC;
```

---

### **14. View Attachments with Task Information**
```sql
SELECT 
    a."FileName",
    a."FileSize",
    a."UploadedAt",
    t."Title" as "TaskTitle",
    u."Email" as "TaskOwner"
FROM "Attachments" a
INNER JOIN "Tasks" t ON a."TaskId" = t."Id"
INNER JOIN "Users" u ON t."UserId" = u."Id"
WHERE a."IsDeleted" = false
ORDER BY a."UploadedAt" DESC;
```

---

### **15. Find Tasks Without Tags**
```sql
SELECT 
    "Id",
    "Title",
    "Status",
    "Priority"
FROM "Tasks"
WHERE "IsDeleted" = false
  AND ("Tags" IS NULL OR jsonb_array_length("Tags") = 0);
```

---

## 📚 Sample Data

The database is seeded with sample data for testing:

### **Users (Password for all: `Password123!`)**
1. john.doe@example.com
2. jane.smith@example.com
3. bob.wilson@example.com

### **Tasks**
- 10 sample tasks with various statuses and priorities
- Mix of pending, in-progress, completed, and cancelled tasks
- Different due dates (past, present, future)
- Various tags like "work", "personal", "urgent", etc.

### **Attachments**
- 3 sample file metadata entries linked to tasks

### **Audit Logs**
- 13+ audit entries tracking all create/update/delete actions

---

## 🚀 Quick Start with DBeaver (Recommended)

### **Step 1: Install DBeaver**
1. Download from: https://dbeaver.io/download/
2. Choose **DBeaver Community Edition** (free)
3. Install for your operating system

### **Step 2: Create Connection**
1. Launch DBeaver
2. Click **"New Database Connection"** (plug icon) or press `Ctrl+Shift+N`
3. Select **PostgreSQL** from the list
4. Click **Next**

### **Step 3: Enter Connection Details**
```
Server: localhost
Port: 5433
Database: TaskTrackerDB
Username: tasktracker_user
Password: TaskTracker123!
```

5. Click **"Test Connection"**
   - First time: DBeaver will offer to download PostgreSQL driver → Click "Download"
   - You should see "Connected" message
6. Click **"Finish"**

### **Step 4: Browse Your Data**
1. Expand the connection tree in left sidebar:
   ```
   TaskTrackerDB
   └── Databases
       └── TaskTrackerDB
           └── Schemas
               └── public
                   └── Tables
                       ├── AuditLogs
                       ├── Attachments
                       ├── Tasks
                       └── Users
   ```

2. **Double-click any table** to view its data
3. **Right-click table** → "View Data" for data browser
4. **Right-click table** → "Properties" to see structure

### **Step 5: Run SQL Queries**
1. Click **"SQL Editor"** icon (or press `Ctrl+]`)
2. Paste any query from this guide
3. Press `Ctrl+Enter` to execute
4. View results in the bottom pane
5. **Export results**: Right-click results → "Export Data" → Choose format

### **Step 6: View ER Diagram (Entity Relationships)**
1. Right-click **"public"** schema
2. Select **"View Diagram"**
3. See visual representation of:
   - All tables
   - Primary keys
   - Foreign key relationships
   - Column types
4. Export diagram: Right-click → "Export Diagram" → PNG/SVG

---

## 💡 Pro Tips

### **Tip 1: Save Favorite Queries**
- Create a SQL script in DBeaver
- Save it in your project: `DatabaseSetupHelp/Queries/MyQueries.sql`
- Open from File → SQL Scripts

### **Tip 2: Use Bookmarks**
- Right-click frequently used tables → "Add to Bookmarks"
- Access quickly from Bookmarks panel

### **Tip 3: Data Export**
- Right-click query results → "Export Data"
- Available formats:
  - CSV (for Excel)
  - JSON (for APIs)
  - SQL INSERT (for migrations)
  - XML, HTML, Markdown, etc.

### **Tip 4: Format SQL**
- Write messy SQL query
- Press `Ctrl+Shift+F` to auto-format
- Makes code readable

### **Tip 5: Execute Explain Plan**
- Add `EXPLAIN ANALYZE` before SELECT
- See query execution plan
- Identify slow queries and missing indexes

**Example:**
```sql
EXPLAIN ANALYZE
SELECT * FROM "Tasks" 
WHERE "Status" = 1 
ORDER BY "DueDate";
```

### **Tip 6: Quick Search**
- Press `Ctrl+F` in any table view
- Search across all visible columns
- Supports regex patterns

---

## 🆘 Troubleshooting

### **Issue: "Connection refused" or "Could not connect"**

**Possible causes & solutions:**

1. **PostgreSQL container not running**
   ```bash
   # Check if container is running
   docker ps
   
   # If not listed, start it
   docker-compose up -d
   
   # Wait 10 seconds for startup
   ```

2. **Wrong port number**
   - Our PostgreSQL runs on port **5433** (not default 5432)
   - Double-check connection settings

3. **Docker not started**
   - Open Docker Desktop
   - Ensure Docker engine is running

---

### **Issue: "Password authentication failed"**

**Solution:** Verify credentials (case-sensitive):
```
Username: tasktracker_user
Password: TaskTracker123!
```

**Common mistakes:**
- ❌ Using `postgres` as username (wrong)
- ❌ Using `tasktracker123!` (wrong case)
- ❌ Extra spaces in password field

---

### **Issue: "Database TaskTrackerDB does not exist"**

**Solution:**
```bash
# Stop and remove containers
docker-compose down

# Start fresh
docker-compose up -d

# Wait 10 seconds, then run migrations
cd TaskTracker.API
dotnet run
# API will auto-apply migrations and seed data
```

---

### **Issue: "No tables visible in database"**

**Cause:** Migrations not applied yet

**Solution:**
```bash
cd TaskTracker.API
dotnet run
# Wait for log message: "Database migrations applied successfully"
# Ctrl+C to stop API
```

Then refresh DBeaver connection (F5).

---

### **Issue: "SSL connection error"**

**Solution:** Disable SSL in connection settings:
1. Edit connection in DBeaver
2. Go to "Driver properties" tab
3. Add property: `ssl = false`
4. Save and reconnect

---

## 🔒 Security Notes

### **Development Environment (Current Setup)**
- ✅ Credentials stored in `docker-compose.yml` and `appsettings.Development.json`
- ✅ Database accessible only on `localhost`
- ✅ Suitable for local development

### **Production Environment (Future)**
- 🚨 **Never** commit production passwords to Git
- 🚨 Use environment variables or Azure Key Vault
- 🚨 Enable SSL/TLS for database connections
- 🚨 Use strong passwords (20+ characters)
- 🚨 Restrict database access by IP whitelist

---

## 📖 Additional Learning Resources

### **PostgreSQL Documentation**
- Official Docs: https://www.postgresql.org/docs/
- PostgreSQL Tutorial: https://www.postgresqltutorial.com/
- JSONB Functions: https://www.postgresql.org/docs/current/functions-json.html

### **Tool Documentation**
- DBeaver Wiki: https://github.com/dbeaver/dbeaver/wiki
- pgAdmin Docs: https://www.pgadmin.org/docs/

### **SQL Practice**
- SQL Practice: https://www.sql-practice.com/
- PostgreSQL Exercises: https://pgexercises.com/

---

## 🎓 Next Steps

After setting up your database connection:

1. ✅ **Browse the sample data** - Understand the schema
2. ✅ **Run the sample queries** - Learn how data is structured
3. ✅ **Create custom queries** - Explore different filters
4. ✅ **View ER diagram** - Visualize relationships
5. ✅ **Test API endpoints in Swagger** - See how API queries the database
6. ✅ **Monitor audit logs** - Track all changes

---

## 📞 Quick Reference Card

```
╔══════════════════════════════════════════════════════════╗
║           TASKTRACKER DATABASE CONNECTION INFO           ║
╠══════════════════════════════════════════════════════════╣
║ Host:         localhost                                  ║
║ Port:         5433                                       ║
║ Database:     TaskTrackerDB                              ║
║ Username:     tasktracker_user                           ║
║ Password:     TaskTracker123!                            ║
╠══════════════════════════════════════════════════════════╣
║ Docker:       docker-compose up -d                       ║
║ API:          http://localhost:5128                      ║
║ Swagger:      http://localhost:5128/swagger              ║
╠══════════════════════════════════════════════════════════╣
║ Sample Users (Password: Password123!)                    ║
║ - john.doe@example.com                                   ║
║ - jane.smith@example.com                                 ║
║ - bob.wilson@example.com                                 ║
╚══════════════════════════════════════════════════════════╝
```

---

**Need help?** Refer to:
- `README.md` - Project overview
- `Phases/Phase1-Backend-Foundation.md` - Implementation details
- `PhaseWalkthroughs/Phase1-Walkthrough.md` - Business logic explanation

Happy querying! 🚀
