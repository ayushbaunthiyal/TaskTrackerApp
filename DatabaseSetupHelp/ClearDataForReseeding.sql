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
