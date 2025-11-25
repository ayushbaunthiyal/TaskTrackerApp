# TaskTracker API - Complete CRUD Request Bodies

Here are ready-to-use request bodies for all API endpoints with actual User IDs from your database.

---

## 📋 **Tasks API - Complete CRUD Examples**

### **Get Valid User IDs First**

Before creating tasks, get actual User IDs from your database:

**Query in DBeaver:**
```sql
SELECT "Id", "Email" FROM "Users" WHERE "IsDeleted" = false;
```

**Expected Result:**
```
f6af7e98-42cb-429d-bc06-72fb286c5846 | jane.smith@example.com
8c5e2d91-7f3a-4b6e-9d8c-1a2b3c4d5e6f | john.doe@example.com
7d4c3b2a-1e9f-8g7h-6i5j-4k3l2m1n0o9p | bob.wilson@example.com
```

*(Actual GUIDs will be different - use the ones from YOUR database)*

---

## 1️⃣ **POST - Create New Task**

### **Example 1: Create Pending Task (Minimal)**

**Endpoint:** `POST /api/tasks`

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Review pull requests",
  "description": "Review and approve pending PRs from team",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-12-05T18:00:00Z",
  "tags": ["code-review", "team"]
}
```

**cURL Command:**
```bash
curl -X 'POST' \
  'http://localhost:5128/api/tasks' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Review pull requests",
  "description": "Review and approve pending PRs from team",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-12-05T18:00:00Z",
  "tags": ["code-review", "team"]
}'
```

---

### **Example 2: Create High Priority Task**

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Fix production bug - Payment gateway timeout",
  "description": "Investigate and fix timeout issues in payment processing. Users reporting failed transactions.",
  "status": 2,
  "priority": 4,
  "dueDate": "2025-11-27T12:00:00Z",
  "tags": ["bug", "critical", "payment", "production"]
}
```

**cURL Command:**
```bash
curl -X 'POST' \
  'http://localhost:5128/api/tasks' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Fix production bug - Payment gateway timeout",
  "description": "Investigate and fix timeout issues in payment processing. Users reporting failed transactions.",
  "status": 2,
  "priority": 4,
  "dueDate": "2025-11-27T12:00:00Z",
  "tags": ["bug", "critical", "payment", "production"]
}'
```

---

### **Example 3: Create Task Without Due Date**

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Research new frontend frameworks",
  "description": "Evaluate React 19, Vue 3.4, and Svelte 5 for next project",
  "status": 1,
  "priority": 1,
  "dueDate": null,
  "tags": ["research", "frontend", "low-priority"]
}
```

---

### **Example 4: Create Task With No Description**

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Team standup meeting",
  "description": "",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-11-27T10:00:00Z",
  "tags": ["meeting", "daily"]
}
```

---

### **Example 5: Create Task With Multiple Tags**

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Implement user authentication",
  "description": "Add JWT-based authentication with refresh tokens, role-based authorization, and password reset functionality",
  "status": 1,
  "priority": 3,
  "dueDate": "2025-12-10T23:59:59Z",
  "tags": ["feature", "authentication", "security", "backend", "phase-2"]
}
```

---

## 2️⃣ **GET - Retrieve Tasks**

### **Example 1: Get All Tasks (Paginated)**

**Endpoint:** `GET /api/tasks?pageNumber=1&pageSize=25`

**cURL Command:**
```bash
curl -X 'GET' \
  'http://localhost:5128/api/tasks?pageNumber=1&pageSize=25' \
  -H 'accept: application/json'
```

---

### **Example 2: Search Tasks by Title/Description**

**Endpoint:** `GET /api/tasks?search=bug`

**cURL Command:**
```bash
curl -X 'GET' \
  'http://localhost:5128/api/tasks?search=bug' \
  -H 'accept: application/json'
```

---

### **Example 3: Filter by Status (InProgress)**

**Endpoint:** `GET /api/tasks?status=2`

**cURL Command:**
```bash
curl -X 'GET' \
  'http://localhost:5128/api/tasks?status=2' \
  -H 'accept: application/json'
```

---

### **Example 4: Filter by Priority (Critical)**

**Endpoint:** `GET /api/tasks?priority=4`

**cURL Command:**
```bash
curl -X 'GET' \
  'http://localhost:5128/api/tasks?priority=4' \
  -H 'accept: application/json'
```

---

### **Example 5: Filter by User**

**Endpoint:** `GET /api/tasks?userId=f6af7e98-42cb-429d-bc06-72fb286c5846`

**cURL Command:**
```bash
curl -X 'GET' \
  'http://localhost:5128/api/tasks?userId=f6af7e98-42cb-429d-bc06-72fb286c5846' \
  -H 'accept: application/json'
```

---

### **Example 6: Filter by Due Date Range**

**Endpoint:** `GET /api/tasks?dueDateFrom=2025-11-27&dueDateTo=2025-12-05`

**cURL Command:**
```bash
curl -X 'GET' \
  'http://localhost:5128/api/tasks?dueDateFrom=2025-11-27&dueDateTo=2025-12-05' \
  -H 'accept: application/json'
```

---

### **Example 7: Combined Filters with Sorting**

**Endpoint:** `GET /api/tasks?status=1&priority=3&sortBy=dueDate&sortDescending=false`

**cURL Command:**
```bash
curl -X 'GET' \
  'http://localhost:5128/api/tasks?status=1&priority=3&sortBy=dueDate&sortDescending=false' \
  -H 'accept: application/json'
```

---

### **Example 8: Get Single Task by ID**

**Endpoint:** `GET /api/tasks/{id}`

**cURL Command (replace {id} with actual task GUID):**
```bash
curl -X 'GET' \
  'http://localhost:5128/api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
  -H 'accept: application/json'
```

---

## 3️⃣ **PUT - Update Task**

### **Example 1: Update Task Status to InProgress**

**Endpoint:** `PUT /api/tasks/{id}`

**Request Body:**
```json
{
  "title": "Review pull requests",
  "description": "Review and approve pending PRs from team",
  "status": 2,
  "priority": 2,
  "dueDate": "2025-12-05T18:00:00Z",
  "tags": ["code-review", "team"]
}
```

**cURL Command:**
```bash
curl -X 'PUT' \
  'http://localhost:5128/api/tasks/YOUR-TASK-ID-HERE' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "title": "Review pull requests",
  "description": "Review and approve pending PRs from team",
  "status": 2,
  "priority": 2,
  "dueDate": "2025-12-05T18:00:00Z",
  "tags": ["code-review", "team"]
}'
```

---

### **Example 2: Mark Task as Completed**

**Request Body:**
```json
{
  "title": "Fix production bug - Payment gateway timeout",
  "description": "Investigate and fix timeout issues in payment processing. RESOLVED: Increased timeout from 30s to 60s.",
  "status": 3,
  "priority": 4,
  "dueDate": "2025-11-27T12:00:00Z",
  "tags": ["bug", "critical", "payment", "production", "resolved"]
}
```

---

### **Example 3: Increase Priority to Critical**

**Request Body:**
```json
{
  "title": "Implement user authentication",
  "description": "Add JWT-based authentication - URGENT: Required for Friday demo",
  "status": 2,
  "priority": 4,
  "dueDate": "2025-11-29T17:00:00Z",
  "tags": ["feature", "authentication", "security", "backend", "urgent"]
}
```

---

### **Example 4: Extend Due Date**

**Request Body:**
```json
{
  "title": "Research new frontend frameworks",
  "description": "Evaluate React 19, Vue 3.4, and Svelte 5 for next project",
  "status": 1,
  "priority": 1,
  "dueDate": "2025-12-20T23:59:59Z",
  "tags": ["research", "frontend", "low-priority"]
}
```

---

### **Example 5: Add More Tags**

**Request Body:**
```json
{
  "title": "Team standup meeting",
  "description": "Daily standup - Discuss blockers and progress",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-11-27T10:00:00Z",
  "tags": ["meeting", "daily", "scrum", "team-sync"]
}
```

---

### **Example 6: Cancel Task**

**Request Body:**
```json
{
  "title": "Research new frontend frameworks",
  "description": "Evaluate React 19, Vue 3.4, and Svelte 5 - CANCELLED: Decided to stick with current stack",
  "status": 4,
  "priority": 1,
  "dueDate": null,
  "tags": ["research", "frontend", "cancelled"]
}
```

---

## 4️⃣ **DELETE - Soft Delete Task**

### **Example: Delete Task**

**Endpoint:** `DELETE /api/tasks/{id}`

**cURL Command:**
```bash
curl -X 'DELETE' \
  'http://localhost:5128/api/tasks/YOUR-TASK-ID-HERE' \
  -H 'accept: */*'
```

**What Happens:**
- Task remains in database but `IsDeleted = true`
- Task no longer appears in GET requests
- Audit log entry created with "Deleted" action
- Can be recovered by manually updating database

---

## 🎯 **Complete Workflow Example**

### **Step 1: Create a Task**

```bash
curl -X 'POST' \
  'http://localhost:5128/api/tasks' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Setup CI/CD pipeline",
  "description": "Configure GitHub Actions for automated testing and deployment",
  "status": 1,
  "priority": 3,
  "dueDate": "2025-12-01T18:00:00Z",
  "tags": ["devops", "automation", "ci-cd"]
}'
```

**Response (201 Created):**
```json
{
  "id": "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Setup CI/CD pipeline",
  "description": "Configure GitHub Actions for automated testing and deployment",
  "status": 1,
  "priority": 3,
  "dueDate": "2025-12-01T18:00:00Z",
  "tags": ["devops", "automation", "ci-cd"],
  "createdAt": "2025-11-26T15:30:00Z",
  "updatedAt": "2025-11-26T15:30:00Z"
}
```

**Copy the `id` from response:** `a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d`

---

### **Step 2: Get the Task**

```bash
curl -X 'GET' \
  'http://localhost:5128/api/tasks/a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d' \
  -H 'accept: application/json'
```

---

### **Step 3: Start Working (Update to InProgress)**

```bash
curl -X 'PUT' \
  'http://localhost:5128/api/tasks/a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "title": "Setup CI/CD pipeline",
  "description": "Configure GitHub Actions for automated testing and deployment - IN PROGRESS",
  "status": 2,
  "priority": 3,
  "dueDate": "2025-12-01T18:00:00Z",
  "tags": ["devops", "automation", "ci-cd", "in-progress"]
}'
```

---

### **Step 4: Complete the Task**

```bash
curl -X 'PUT' \
  'http://localhost:5128/api/tasks/a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "title": "Setup CI/CD pipeline",
  "description": "Configure GitHub Actions for automated testing and deployment - COMPLETED: Pipeline running successfully",
  "status": 3,
  "priority": 3,
  "dueDate": "2025-12-01T18:00:00Z",
  "tags": ["devops", "automation", "ci-cd", "completed"]
}'
```

---

### **Step 5: Verify Audit Trail**

**Query in DBeaver:**
```sql
SELECT 
    "Action",
    "EntityType",
    "Timestamp",
    "Details"
FROM "AuditLogs"
WHERE "EntityId" = 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d'
ORDER BY "Timestamp";
```

**Expected Result:**
```
Created  | TaskItem | 2025-11-26 15:30:00 | ""
Updated  | TaskItem | 2025-11-26 15:35:00 | {"Status":{"Old":1,"New":2},...}
Updated  | TaskItem | 2025-11-26 16:00:00 | {"Status":{"Old":2,"New":3},...}
```

---

### **Step 6: Delete the Task (Optional)**

```bash
curl -X 'DELETE' \
  'http://localhost:5128/api/tasks/a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d' \
  -H 'accept: */*'
```

---

## 🧪 **Validation Test Cases**

### **Test 1: Missing Required Field (Title)**

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "description": "Task without title",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-12-01T18:00:00Z",
  "tags": ["test"]
}
```

**Expected Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["Title is required"]
  }
}
```

---

### **Test 2: Title Too Long**

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "This is a very long title that exceeds the maximum allowed length of 200 characters. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam quis nostrud.",
  "description": "Testing validation",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-12-01T18:00:00Z",
  "tags": ["test"]
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "Title": ["Title must be between 1 and 200 characters"]
  }
}
```

---

### **Test 3: Invalid Status**

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Invalid status task",
  "description": "Testing validation",
  "status": 99,
  "priority": 2,
  "dueDate": "2025-12-01T18:00:00Z",
  "tags": ["test"]
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "Status": ["Status must be between 1 and 4"]
  }
}
```

---

### **Test 4: Invalid Priority**

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Invalid priority task",
  "description": "Testing validation",
  "status": 1,
  "priority": 0,
  "dueDate": "2025-12-01T18:00:00Z",
  "tags": ["test"]
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "Priority": ["Priority must be between 1 and 4"]
  }
}
```

---

### **Test 5: Past Due Date**

```json
{
  "userId": "f6af7e98-42cb-429d-bc06-72fb286c5846",
  "title": "Task with past due date",
  "description": "Testing validation",
  "status": 1,
  "priority": 2,
  "dueDate": "2020-01-01T00:00:00Z",
  "tags": ["test"]
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "DueDate": ["Due date must be in the future"]
  }
}
```

---

### **Test 6: Non-Existent User**

```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "title": "Task for non-existent user",
  "description": "Testing validation",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-12-01T18:00:00Z",
  "tags": ["test"]
}
```

**Expected Response (404 Not Found):**
```json
{
  "error": "User with ID 00000000-0000-0000-0000-000000000000 not found"
}
```

---

## 📚 **Quick Reference**

### **Base URL:**
```
http://localhost:5128
```

### **Status Values:**
- `1` = Pending
- `2` = InProgress
- `3` = Completed
- `4` = Cancelled

### **Priority Values:**
- `1` = Low
- `2` = Medium
- `3` = High
- `4` = Critical

### **Available Sort Fields:**
- `title`
- `status`
- `priority`
- `dueDate`
- `createdAt`
- `updatedAt`

### **Pagination Defaults:**
- Default page size: 25
- Maximum page size: 100
- Minimum page number: 1

### **Filter Parameters:**
- `search` - Searches in title and description
- `status` - Filter by status (1-4)
- `priority` - Filter by priority (1-4)
- `userId` - Filter by user GUID
- `tag` - Filter by single tag
- `dueDateFrom` - Tasks due after this date (YYYY-MM-DD)
- `dueDateTo` - Tasks due before this date (YYYY-MM-DD)
- `sortBy` - Sort field name
- `sortDescending` - true/false
- `pageNumber` - Page number (min: 1)
- `pageSize` - Page size (max: 100)

---

## 🔧 **Using Swagger UI**

1. Navigate to: `http://localhost:5128/swagger`
2. Click on any endpoint to expand it
3. Click "Try it out" button
4. Fill in the parameters/request body
5. Click "Execute"
6. View the response below

---

## 💡 **Tips**

1. **Get User IDs:** Always query the database first to get valid User GUIDs
2. **Copy Task IDs:** After creating a task, copy the `id` from the response for update/delete operations
3. **Test Validation:** Try the validation test cases to understand error responses
4. **Check Audit Logs:** After CRUD operations, query the `AuditLogs` table to see the audit trail
5. **Use Swagger:** For quick testing, use the built-in Swagger UI instead of writing cURL commands

---

## 🎯 **Common Scenarios**

### **Scenario 1: Create and Track a Bug Fix**
1. Create task with status=1 (Pending), priority=4 (Critical), tags=["bug", "production"]
2. Update to status=2 (InProgress) when starting work
3. Update to status=3 (Completed) when fixed
4. Check audit logs to see the complete timeline

### **Scenario 2: Get Overdue Tasks**
```
GET /api/tasks?dueDateTo=2025-11-26&status=1,2&sortBy=dueDate
```
(Replace date with today's date)

### **Scenario 3: Get User's Task Summary**
```
GET /api/tasks?userId=USER_GUID&status=1&sortBy=priority&sortDescending=true
```
Shows user's pending tasks ordered by priority

### **Scenario 4: Weekly Planning**
```
GET /api/tasks?dueDateFrom=2025-11-26&dueDateTo=2025-12-02&sortBy=dueDate
```
Shows all tasks due this week

---

**Last Updated:** November 26, 2025
