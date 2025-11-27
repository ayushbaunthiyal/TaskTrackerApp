-- Manual Task Seeding Script for TaskTracker Database
-- Run this script in DBeaver or pgAdmin if tasks weren't seeded automatically

-- First, get the User IDs (copy these values for use below)
SELECT "Id", "Email" FROM "Users" ORDER BY "Email";

-- After getting User IDs, replace the placeholders below with actual GUIDs
-- Then run the INSERT statements

-- INSTRUCTIONS:
-- 1. Run the SELECT above to get User IDs
-- 2. Copy the three GUID values
-- 3. Replace 'USER1_GUID_HERE', 'USER2_GUID_HERE', 'USER3_GUID_HERE' below
-- 4. Run the INSERT statements

-- Insert 10 sample tasks
INSERT INTO "Tasks" ("Id", "UserId", "Title", "Description", "Status", "Priority", "DueDate", "Tags", "CreatedAt", "UpdatedAt", "IsDeleted")
VALUES
-- Task 1 - Assigned to first user (john.doe@example.com)
(
    gen_random_uuid(),
    'USER1_GUID_HERE',  -- Replace with actual GUID from john.doe@example.com
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

-- Task 2 - Assigned to first user
(
    gen_random_uuid(),
    'USER1_GUID_HERE',  -- Replace with actual GUID from john.doe@example.com
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

-- Task 3 - Assigned to second user (jane.smith@example.com)
(
    gen_random_uuid(),
    'USER2_GUID_HERE',  -- Replace with actual GUID from jane.smith@example.com
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

-- Task 4 - Assigned to second user
(
    gen_random_uuid(),
    'USER2_GUID_HERE',  -- Replace with actual GUID from jane.smith@example.com
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

-- Task 5 - Assigned to third user (bob.wilson@example.com)
(
    gen_random_uuid(),
    'USER3_GUID_HERE',  -- Replace with actual GUID from bob.wilson@example.com
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

-- Task 6 - Assigned to third user
(
    gen_random_uuid(),
    'USER3_GUID_HERE',  -- Replace with actual GUID from bob.wilson@example.com
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

-- Task 7 - Assigned to first user
(
    gen_random_uuid(),
    'USER1_GUID_HERE',  -- Replace with actual GUID from john.doe@example.com
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

-- Task 8 - Assigned to second user
(
    gen_random_uuid(),
    'USER2_GUID_HERE',  -- Replace with actual GUID from jane.smith@example.com
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

-- Task 9 - Assigned to third user
(
    gen_random_uuid(),
    'USER3_GUID_HERE',  -- Replace with actual GUID from bob.wilson@example.com
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

-- Task 10 - Assigned to first user
(
    gen_random_uuid(),
    'USER1_GUID_HERE',  -- Replace with actual GUID from john.doe@example.com
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
