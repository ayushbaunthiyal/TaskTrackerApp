# Phase 1 Business Logic Walkthrough

This document explains the business logic implemented in Phase 1 for developers who may be new to PostgreSQL or the Task Tracker application architecture.

## 📋 Core Business Entities

### 1. **Users**
- Each user has an email, password (hashed with BCrypt), and timestamps
- Users can own multiple tasks
- Sample data: 3 users (john.doe@example.com, jane.smith@example.com, bob.wilson@example.com)
- Password for all sample users: `Password123!`

### 2. **Tasks (TaskItem)**
The main entity of the application. Each task has:
- **Title** & **Description**: What the task is about
- **Status**: Current state of the task
  - `Pending (1)` - Not started yet
  - `InProgress (2)` - Currently being worked on
  - `Completed (3)` - Finished
  - `Cancelled (4)` - No longer needed
- **Priority**: How important the task is
  - `Low (1)` - Can wait
  - `Medium (2)` - Normal importance
  - `High (3)` - Important
  - `Critical (4)` - Urgent, must do now
- **DueDate**: When the task should be completed (nullable)
- **Tags**: Categories/labels stored as JSON array (e.g., ["work", "urgent", "meeting"])
- **Ownership**: Each task belongs to one user (UserId)

### 3. **Attachments**
- Files linked to tasks (like screenshots, documents)
- Stores: filename, file size, file path, upload date
- Each attachment belongs to one task
- Currently stores metadata only (actual file upload to be implemented in Phase 3)

### 4. **Audit Logs**
- Automatic tracking of all changes
- Records: who did what, when, and to which entity
- Examples: "User 1 created Task 5", "User 2 updated Task 3"
- Stores change details as JSON for full history

## 🔄 Business Logic Flow

### **Creating a Task**
```
User Request → Validation → Save to Database → Log Audit Entry → Return Created Task
```

**Step-by-step process:**

1. User sends task data (title, description, priority, etc.)
2. **Validation** checks:
   - Title is required (1-200 characters)
   - Description is optional (max 2000 characters)
   - Status must be valid enum value (1-4)
   - Priority must be valid enum value (1-4)
   - DueDate must be in the future (if provided)
   - Maximum 10 tags allowed
3. Task is saved to PostgreSQL database
4. **Audit Interceptor** automatically logs "TaskCreated" action
5. API returns the created task with generated ID and timestamps

**Example Request:**
```json
POST /api/tasks
{
  "userId": "guid-here",
  "title": "Prepare quarterly report",
  "description": "Financial report for Q4 2024",
  "status": 1,
  "priority": 3,
  "dueDate": "2025-12-01T17:00:00Z",
  "tags": ["work", "finance", "quarterly"]
}
```

**Example Response:**
```json
{
  "id": "new-guid",
  "userId": "guid-here",
  "title": "Prepare quarterly report",
  "description": "Financial report for Q4 2024",
  "status": 1,
  "priority": 3,
  "dueDate": "2025-12-01T17:00:00Z",
  "tags": ["work", "finance", "quarterly"],
  "createdAt": "2025-11-26T10:00:00Z",
  "updatedAt": "2025-11-26T10:00:00Z"
}
```

### **Getting Tasks (with Search & Filters)**
```
Request with Filters → Query Database → Apply Pagination → Return Results
```

**Example:** `GET /api/tasks?status=2&priority=3&searchTerm=meeting&pageNumber=1&pageSize=25`

This means: "Show me InProgress tasks with High priority that contain 'meeting' in title/description, first 25 results"

**The system processes this as:**
1. Starts with all non-deleted tasks (soft delete filter applied automatically)
2. Filters by status (InProgress = 2)
3. Filters by priority (High = 3)
4. Searches title/description for "meeting" (case-insensitive)
5. Sorts results (configurable via sortBy parameter, default: CreatedAt)
6. Takes page 1 with 25 items (skip = 0, take = 25)
7. Calculates total count for pagination metadata
8. Returns paginated response

**Available Filter Parameters:**
- `searchTerm` - Search in title and description
- `status` - Filter by TaskStatus (1-4)
- `priority` - Filter by TaskPriority (1-4)
- `tag` - Filter by specific tag (exact match)
- `dueDateFrom` - Tasks due after this date
- `dueDateTo` - Tasks due before this date
- `sortBy` - Field to sort by (Title, Status, Priority, DueDate, CreatedAt, UpdatedAt)
- `sortDescending` - Sort direction (true/false, default: false)
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 25, max: 100)

**Example Paginated Response:**
```json
{
  "items": [
    { /* task 1 */ },
    { /* task 2 */ },
    // ... 25 tasks total
  ],
  "pageNumber": 1,
  "pageSize": 25,
  "totalCount": 150,
  "totalPages": 6,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### **Updating a Task**
```
Request → Find Existing Task → Validate → Update Fields → Log Audit → Return Updated Task
```

**Process:**
1. Find task by ID (returns 404 if not found or soft-deleted)
2. Validate new data using FluentValidation rules
3. Update only the fields provided in the request
4. **Audit Interceptor** automatically logs "TaskUpdated" with old/new values in JSON format
5. `UpdatedAt` timestamp automatically set to current time
6. Return updated task

**Example Request:**
```json
PUT /api/tasks/{id}
{
  "status": 2,  // Change to InProgress
  "priority": 4 // Increase to Critical
}
```

**What gets logged in AuditLog:**
```json
{
  "userId": "user-guid",
  "action": "TaskUpdated",
  "entityType": "TaskItem",
  "entityId": "task-guid",
  "timestamp": "2025-11-26T10:30:00Z",
  "details": "{\"Status\":{\"Old\":1,\"New\":2},\"Priority\":{\"Old\":3,\"New\":4},\"UpdatedAt\":{\"Old\":\"2025-11-26T10:00:00Z\",\"New\":\"2025-11-26T10:30:00Z\"}}"
}
```

### **Soft Delete**
```
Delete Request → Mark as Deleted → Log Audit → Return Success (204 No Content)
```

**Instead of actually deleting from database:**
1. Set `IsDeleted = true` flag
2. Set `UpdatedAt = current timestamp`
3. Audit log records "TaskDeleted" action
4. Task no longer appears in normal queries (automatically filtered out)
5. Related attachments cascade to soft delete as well

**Why soft delete?**
- ✅ Can recover accidentally deleted tasks
- ✅ Maintains complete audit trail
- ✅ Preserves historical data and relationships
- ✅ Supports compliance requirements
- ✅ No broken foreign key references

**How it works:**
Entity Framework applies a global query filter to all entities inheriting from `BaseEntity`:
```csharp
modelBuilder.Entity<BaseEntity>()
    .HasQueryFilter(e => !e.IsDeleted);
```

This means every query automatically adds `WHERE IsDeleted = false` - you never see deleted items unless you explicitly disable the filter.

## 🗄️ PostgreSQL Specifics

### **JSONB Storage for Tags**
```sql
Tags column type: jsonb (JSON Binary)
```

**Example stored value:**
```json
["work", "urgent", "meeting"]
```

**Benefits:**
- **Flexible**: No fixed number of tags, no separate tags table needed
- **Queryable**: Can search within JSON using PostgreSQL operators
- **Efficient**: JSONB is stored in binary format, indexed, and very fast
- **Schema-less**: Can change tag structure without migrations

**Query example in PostgreSQL:**
```sql
-- Find tasks with "urgent" tag
SELECT * FROM "Tasks" 
WHERE "Tags" @> '["urgent"]'::jsonb;
```

**In Entity Framework:**
```csharp
// Filter by tag using Contains
tasks.Where(t => t.Tags.Contains(filterDto.Tag))
```

### **Indexes for Performance**

We created indexes on frequently queried columns to speed up searches:

```sql
-- UserId index - to quickly find all tasks for a user
CREATE INDEX "IX_Tasks_UserId" ON "Tasks" ("UserId");

-- Status index - to filter by status
CREATE INDEX "IX_Tasks_Status" ON "Tasks" ("Status");

-- Priority index - to filter by priority
CREATE INDEX "IX_Tasks_Priority" ON "Tasks" ("Priority");

-- DueDate index - to sort/filter by due date
CREATE INDEX "IX_Tasks_DueDate" ON "Tasks" ("DueDate");

-- IsDeleted index - to exclude deleted items efficiently
CREATE INDEX "IX_Tasks_IsDeleted" ON "Tasks" ("IsDeleted");
```

**Without indexes:**
- Database scans entire table (slow for large datasets)

**With indexes:**
- Database uses B-tree lookup (very fast, even with millions of rows)

### **Global Query Filters**

Automatically applied to all queries to exclude soft-deleted items:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply to all entities inheriting from BaseEntity
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(e => !((BaseEntity)e).IsDeleted);
        }
    }
}
```

**Result:** You never see soft-deleted items in normal queries without explicitly disabling the filter.

### **Entity Relationships & Cascade Behavior**

```
User (1) ──────→ (Many) Tasks
Task (1) ──────→ (Many) Attachments
User (1) ──────→ (Many) AuditLogs (nullable)
```

**Cascade Delete Configuration:**
- User deleted → Tasks soft deleted (Cascade)
- Task deleted → Attachments soft deleted (Cascade)
- User deleted → AuditLogs keep reference but UserId set to null (SetNull)

## 🔍 Audit Logging (Automatic)

The **AuditInterceptor** runs on every database save operation:

```
SaveChanges() → Interceptor checks changes → Creates AuditLog entries
```

### **What it tracks:**

1. **Created Entities:**
   - Action: "Created"
   - Details: Empty (full entity visible via EntityId)

2. **Updated Entities:**
   - Action: "Updated"
   - Details: JSON with old/new values for changed properties
   - Automatically excludes UpdatedAt changes (too noisy)

3. **Deleted Entities:**
   - Action: "Deleted"
   - Details: Empty

### **Example Audit Log Entries:**

**Task Created:**
```json
{
  "id": "audit-guid-1",
  "userId": "user-guid",
  "action": "Created",
  "entityType": "TaskItem",
  "entityId": "task-guid",
  "timestamp": "2025-11-26T10:00:00Z",
  "details": ""
}
```

**Task Updated:**
```json
{
  "id": "audit-guid-2",
  "userId": "user-guid",
  "action": "Updated",
  "entityType": "TaskItem",
  "entityId": "task-guid",
  "timestamp": "2025-11-26T10:30:00Z",
  "details": "{\"Status\":{\"Old\":1,\"New\":2},\"Priority\":{\"Old\":2,\"New\":4}}"
}
```

**Task Deleted:**
```json
{
  "id": "audit-guid-3",
  "userId": "user-guid",
  "action": "Deleted",
  "entityType": "TaskItem",
  "entityId": "task-guid",
  "timestamp": "2025-11-26T11:00:00Z",
  "details": ""
}
```

### **How it works:**

The `AuditInterceptor` overrides `SaveChangesAsync`:

```csharp
public override async ValueTask<int> SaveChangesAsync(...)
{
    var entries = ChangeTracker.Entries<BaseEntity>()
        .Where(e => e.State == EntityState.Added || 
                    e.State == EntityState.Modified || 
                    e.State == EntityState.Deleted);
    
    foreach (var entry in entries)
    {
        // Set timestamps
        if (entry.State == EntityState.Added)
            entry.Entity.CreatedAt = DateTime.UtcNow;
        
        entry.Entity.UpdatedAt = DateTime.UtcNow;
        
        // Create audit log
        var auditLog = CreateAuditLog(entry);
        await _auditLogRepository.AddAsync(auditLog);
    }
    
    return await base.SaveChangesAsync(...);
}
```

## 📊 Pagination Logic

### **Configuration:**
- **Default page size:** 25 items
- **Maximum page size:** 100 items (prevents performance issues)
- **Minimum page number:** 1

### **Implementation:**

```csharp
// Ensure valid page parameters
pageNumber = pageNumber < 1 ? 1 : pageNumber;
pageSize = pageSize > 100 ? 100 : pageSize;
pageSize = pageSize < 1 ? 25 : pageSize;

// Calculate skip/take
var skip = (pageNumber - 1) * pageSize;
var take = pageSize;

// Apply to query
var items = await query
    .Skip(skip)
    .Take(take)
    .ToListAsync();

// Calculate pagination metadata
var totalCount = await query.CountAsync();
var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
```

### **Pagination Examples:**

**Page 1, Size 25:**
- Skip: 0
- Take: 25
- Returns: Items 1-25

**Page 2, Size 25:**
- Skip: 25
- Take: 25
- Returns: Items 26-50

**Page 3, Size 50:**
- Skip: 100
- Take: 50
- Returns: Items 101-150

### **Response Structure:**

```json
{
  "items": [/* array of tasks */],
  "pageNumber": 2,
  "pageSize": 25,
  "totalCount": 150,
  "totalPages": 6,
  "hasPreviousPage": true,
  "hasNextPage": true
}
```

**Calculated properties:**
- `totalPages` = Math.Ceiling(totalCount / pageSize)
- `hasPreviousPage` = pageNumber > 1
- `hasNextPage` = pageNumber < totalPages

## 🛡️ Validation Rules

### **Task Creation/Update Validation (FluentValidation):**

```csharp
// Title validation
RuleFor(x => x.Title)
    .NotEmpty().WithMessage("Title is required")
    .Length(1, 200).WithMessage("Title must be between 1 and 200 characters");

// Description validation
RuleFor(x => x.Description)
    .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

// Status validation
RuleFor(x => x.Status)
    .IsInEnum().WithMessage("Invalid task status");

// Priority validation
RuleFor(x => x.Priority)
    .IsInEnum().WithMessage("Invalid task priority");

// DueDate validation
RuleFor(x => x.DueDate)
    .GreaterThan(DateTime.UtcNow)
    .When(x => x.DueDate.HasValue)
    .WithMessage("Due date must be in the future");

// Tags validation
RuleFor(x => x.Tags)
    .Must(tags => tags == null || tags.Length <= 10)
    .WithMessage("Maximum 10 tags allowed");
```

### **Validation Error Response:**

When validation fails, the API returns a `400 Bad Request` with detailed error information:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": [
      "Title is required",
      "Title must be between 1 and 200 characters"
    ],
    "DueDate": [
      "Due date must be in the future"
    ],
    "Tags": [
      "Maximum 10 tags allowed"
    ]
  }
}
```

### **When validation runs:**
1. **Before creating a task** - ValidateAsync(createTaskDto)
2. **Before updating a task** - ValidateAsync(updateTaskDto)
3. Automatic via ASP.NET Core middleware
4. Fails fast - no database hit if validation fails

## 🏗️ Architecture Pattern (Clean Architecture)

```
┌─────────────────────────────────────┐
│     API Layer (Controllers)         │  ← HTTP Endpoints
│  - TasksController                  │
│  - Middleware                       │
└──────────────┬──────────────────────┘
               │ calls
               ▼
┌─────────────────────────────────────┐
│  Application Layer (Business Logic) │  ← Use Cases
│  - Services (TaskService)           │
│  - DTOs, Validators                 │
│  - Interfaces                       │
└──────────────┬──────────────────────┘
               │ calls
               ▼
┌─────────────────────────────────────┐
│ Infrastructure (Data Access)        │  ← Database
│  - Repositories                     │
│  - DbContext, Configurations        │
│  - Migrations                       │
└──────────────┬──────────────────────┘
               │ uses
               ▼
┌─────────────────────────────────────┐
│   Domain Layer (Core Entities)      │  ← Business Models
│  - Entities (User, Task, etc.)      │
│  - Enums (Status, Priority)         │
│  - Base Classes                     │
└─────────────────────────────────────┘
```

### **Dependency Flow:**
- API → Application → Infrastructure → Domain
- Domain has NO dependencies (pure business logic)
- Infrastructure depends on Domain (implements repositories)
- Application depends on Domain (uses entities, defines interfaces)
- API depends on Application (calls services)

### **Benefits:**
- ✅ **Testable**: Business logic independent of database/framework
- ✅ **Maintainable**: Clear separation of concerns
- ✅ **Flexible**: Can swap PostgreSQL for SQL Server without changing business logic
- ✅ **Scalable**: Easy to add new features without breaking existing code
- ✅ **SOLID principles**: Each layer has single responsibility

## 🔧 Current Limitations (To be added in future phases)

### **Security (Phase 2)**
- ❌ No authentication - anyone can access any endpoint
- ❌ No authorization - users can see/modify all tasks (not just their own)
- ❌ No JWT tokens
- ❌ No password validation on registration
- ❌ No rate limiting - can spam requests

### **File Management (Phase 3)**
- ❌ No actual file upload - only metadata storage
- ❌ No file download endpoint
- ❌ No file validation (size, type)
- ❌ No cloud storage integration

### **Background Processing (Phase 4)**
- ❌ No reminder notifications
- ❌ No scheduled tasks
- ❌ No email sending
- ❌ No background worker service

### **Frontend (Phase 5+)**
- ❌ No user interface - only API endpoints
- ❌ No React application
- ❌ No authentication pages

## 🎯 Sample Workflow: Complete Task Lifecycle

Let's walk through a complete example of creating, updating, and managing a task.

### **Step 1: Create a Task**

**Request:**
```http
POST /api/tasks HTTP/1.1
Content-Type: application/json

{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Prepare quarterly report",
  "description": "Financial report for Q4 2024",
  "status": 1,
  "priority": 3,
  "dueDate": "2025-12-01T17:00:00Z",
  "tags": ["work", "finance", "quarterly"]
}
```

**Response:** `201 Created`
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Prepare quarterly report",
  "description": "Financial report for Q4 2024",
  "status": 1,
  "priority": 3,
  "dueDate": "2025-12-01T17:00:00Z",
  "tags": ["work", "finance", "quarterly"],
  "createdAt": "2025-11-26T10:00:00Z",
  "updatedAt": "2025-11-26T10:00:00Z"
}
```

**What happened in the database:**
1. New row inserted in `Tasks` table
2. `CreatedAt` and `UpdatedAt` timestamps set automatically
3. Audit log entry created: "Created TaskItem"

---

### **Step 2: Search for Tasks**

**Request:**
```http
GET /api/tasks?status=1&priority=3&sortBy=DueDate&pageSize=10 HTTP/1.1
```

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "title": "Prepare quarterly report",
      "status": 1,
      "priority": 3,
      "dueDate": "2025-12-01T17:00:00Z",
      // ... other fields
    }
    // ... more matching tasks
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 5,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

### **Step 3: Start Working on Task**

**Request:**
```http
PUT /api/tasks/7c9e6679-7425-40de-944b-e07fc1f90ae7 HTTP/1.1
Content-Type: application/json

{
  "status": 2
}
```

**Response:** `200 OK`
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 2,  // Changed to InProgress
  "updatedAt": "2025-11-26T11:30:00Z",  // Timestamp updated
  // ... other fields unchanged
}
```

**What happened:**
1. Task status changed from Pending (1) to InProgress (2)
2. `UpdatedAt` timestamp updated automatically
3. Audit log created with old/new status values

---

### **Step 4: Mark as Complete**

**Request:**
```http
PUT /api/tasks/7c9e6679-7425-40de-944b-e07fc1f90ae7 HTTP/1.1
Content-Type: application/json

{
  "status": 3
}
```

**Response:** `200 OK`
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 3,  // Completed
  "updatedAt": "2025-11-26T14:00:00Z",
  // ... other fields
}
```

---

### **Step 5: Delete Task (Soft Delete)**

**Request:**
```http
DELETE /api/tasks/7c9e6679-7425-40de-944b-e07fc1f90ae7 HTTP/1.1
```

**Response:** `204 No Content`

**What happened:**
1. `IsDeleted` flag set to `true`
2. Task no longer appears in GET /api/tasks queries
3. Task data still in database (can be recovered)
4. Audit log records deletion

---

### **Step 6: View Audit Trail**

**Request:**
```http
GET /api/auditlogs?entityId=7c9e6679-7425-40de-944b-e07fc1f90ae7 HTTP/1.1
```

**Response:** Shows complete history
```json
[
  {
    "action": "Created",
    "timestamp": "2025-11-26T10:00:00Z",
    "details": ""
  },
  {
    "action": "Updated",
    "timestamp": "2025-11-26T11:30:00Z",
    "details": "{\"Status\":{\"Old\":1,\"New\":2}}"
  },
  {
    "action": "Updated",
    "timestamp": "2025-11-26T14:00:00Z",
    "details": "{\"Status\":{\"Old\":2,\"New\":3}}"
  },
  {
    "action": "Deleted",
    "timestamp": "2025-11-26T15:00:00Z",
    "details": ""
  }
]
```

---

## 📚 Key Takeaways

### **For PostgreSQL Beginners:**
1. **JSONB is powerful** - Use it for flexible schema data like tags
2. **Indexes matter** - Add them to frequently queried columns
3. **Soft deletes are safer** - Mark as deleted instead of removing data
4. **Relationships cascade** - Configure cascade behavior carefully

### **For .NET Developers:**
1. **Clean Architecture pays off** - Separation makes testing/maintenance easier
2. **FluentValidation is elegant** - Much cleaner than manual validation
3. **Interceptors are magic** - Auto-handle timestamps, auditing, etc.
4. **Global query filters** - DRY principle for common filters like soft delete

### **For API Designers:**
1. **Pagination is mandatory** - Never return unbounded lists
2. **ProblemDetails standard** - Use RFC 7807 for consistent errors
3. **Filter + Sort + Search** - Give users flexible query options
4. **Audit everything** - Business users love detailed history

### **For Team Collaboration:**
1. **Document as you build** - This walkthrough helps onboarding
2. **Seed data is crucial** - Makes testing/demos easier
3. **Swagger is your friend** - Auto-generated, always up-to-date docs
4. **Phase approach works** - Build incrementally, validate each phase

---

## 🚀 Next Steps

Now that Phase 1 is complete and you understand the business logic:

1. **Test the API** - Use Swagger at http://localhost:5128/swagger
2. **Explore the database** - Connect to PostgreSQL and see the tables
3. **Read the code** - Follow a request from Controller → Service → Repository
4. **Plan Phase 2** - Authentication and authorization
5. **Consider edge cases** - What if user deletes task with attachments?

**Ready for Phase 2?** We'll add:
- JWT authentication
- User registration/login
- Authorization (users can only modify their own tasks)
- Password change functionality
- Protected endpoints

---

## 📞 Questions & Troubleshooting

### **Q: How do I see soft-deleted tasks?**
A: You need to disable the global query filter in your repository query:
```csharp
var deletedTasks = await _context.Tasks
    .IgnoreQueryFilters()
    .Where(t => t.IsDeleted)
    .ToListAsync();
```

### **Q: How do I restore a soft-deleted task?**
A: Set `IsDeleted = false` and save:
```csharp
task.IsDeleted = false;
await _context.SaveChangesAsync();
```

### **Q: Can I query tags with complex conditions?**
A: Yes! PostgreSQL JSONB supports many operators:
```csharp
// Contains tag
tasks.Where(t => t.Tags.Contains("urgent"))

// In EF Core SQL, this becomes:
// WHERE "Tags" @> '["urgent"]'::jsonb
```

### **Q: Why use Guid for IDs instead of int?**
A: 
- Globally unique (good for distributed systems)
- No sequential guessing
- Can generate client-side
- Better for merging databases

### **Q: What happens if validation fails mid-save?**
A: FluentValidation runs BEFORE database operations, so database stays consistent.

### **Q: How do I test without Swagger?**
A: Use curl, Postman, or any HTTP client:
```bash
curl -X GET "http://localhost:5128/api/tasks?pageSize=5" -H "accept: application/json"
```

---

**End of Phase 1 Walkthrough** 🎉
