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
