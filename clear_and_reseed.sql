-- Clear existing data to allow reseeding
DELETE FROM "Attachments";
DELETE FROM "AuditLogs" WHERE "EntityType" = 'TaskItem' OR "EntityType" = 'Attachment';
DELETE FROM "Tasks";

-- The application will automatically reseed data on next startup
