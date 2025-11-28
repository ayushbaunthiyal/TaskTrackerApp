# TaskTracker - Architecture Documentation

> **Version:** 2.0  
> **Last Updated:** November 28, 2025  
> **Status:** Production Ready (Phase 5 Complete)

## 📑 Table of Contents

1. [System Overview](#-system-overview)
2. [C4 Architecture Diagrams](#-c4-architecture-diagrams)
3. [UML Diagrams](#-uml-diagrams)
4. [Database Architecture](#-database-architecture)
5. [Sequence Diagrams](#-sequence-diagrams)
6. [Security Architecture](#-security-architecture)
7. [Deployment Architecture](#-deployment-architecture)
8. [Technology Stack](#-technology-stack)
9. [Monitoring & Observability](#-monitoring--observability)

---

## 📋 System Overview

TaskTracker is a **production-ready, enterprise-grade task management application** built using **Clean Architecture** principles with a focus on scalability, maintainability, and security.

### Core Components

| Component | Technology | Purpose | Port | Health Check |
|-----------|-----------|---------|------|--------------|
| **Web UI** | React 18 + TypeScript | User interface & client-side routing | 3000 | N/A (static) |
| **REST API** | .NET 9 + ASP.NET Core | Business logic, auth, data access | 5128 | /health |
| **Worker Service** | .NET 9 Hosted Service | Background email reminders | 5129 | /health |
| **Database** | PostgreSQL 16 | Persistent storage with JSONB | 5433 | pg_isready |

### External Dependencies

- **Mailgun API v3** - Transactional email delivery service

### Key Features

- ✅ **113 automated tests** (Unit + Integration + Repository)
- ✅ **JWT Authentication** with refresh tokens
- ✅ **Rate Limiting** (3-tier policy: per-user, per-IP auth, per-IP strict)
- ✅ **Audit Trail** for all operations
- ✅ **File Attachments** with ownership validation
- ✅ **Email Reminders** (24-hour lookahead with idempotency)
- ✅ **Prometheus Metrics** for observability
- ✅ **Structured Logging** with Serilog
- ✅ **Docker Compose** orchestration

### Key Architectural Principles

- ✅ Clean Architecture (Domain → Application → Infrastructure → API)
- ✅ SOLID principles throughout
- ✅ Domain-Driven Design (DDD) patterns
- ✅ Repository pattern with Unit of Work
- ✅ Dependency Injection (DI)
- ✅ Asynchronous operations (async/await)
- ✅ Event-driven audit logging

---

## 🏗️ C4 Architecture Diagrams

### Level 1: System Context

```mermaid
graph TB
    User[👤 End Users<br/>Web Browsers<br/>Desktop/Mobile]
    Admin[👨‍💼 Administrators<br/>System Monitoring]
    
    subgraph "TaskTracker System Boundary"
        UI[🖥️ React Web UI<br/>SPA + Nginx<br/>Port 3000]
        API[⚙️ .NET REST API<br/>JWT Auth + Business Logic<br/>Port 5128]
        Worker[🔄 Worker Service<br/>Background Jobs<br/>Port 5129]
        DB[(🗄️ PostgreSQL 16<br/>Relational Database<br/>Port 5433)]
    end
    
    Mailgun[📧 Mailgun<br/>Email Service Provider<br/>api.mailgun.net]
    Prometheus[📊 Prometheus<br/>Metrics Collector<br/>Optional]
    
    User -->|HTTPS| UI
    Admin -->|Metrics Query| Prometheus
    UI -->|REST API<br/>JSON + JWT| API
    API -->|SQL Queries<br/>Npgsql| DB
    Worker -->|SQL Queries<br/>Npgsql| DB
    Worker -->|Send Emails<br/>HTTPS POST| Mailgun
    API -.->|Expose Metrics| Prometheus
    Worker -.->|Expose Metrics| Prometheus
    
    style UI fill:#61dafb,stroke:#333,stroke-width:3px,color:#000
    style API fill:#512bd4,stroke:#333,stroke-width:3px,color:#fff
    style Worker fill:#512bd4,stroke:#333,stroke-width:3px,color:#fff
    style DB fill:#336791,stroke:#333,stroke-width:3px,color:#fff
    style Mailgun fill:#f06529,stroke:#333,stroke-width:2px,color:#fff
    style Prometheus fill:#e6522c,stroke:#333,stroke-width:2px,color:#fff
```

**System Responsibilities:**

- **React UI**: Renders task lists, forms, authentication screens; handles client-side validation
- **API**: Authenticates users, validates input, enforces authorization, persists data
- **Worker**: Sends scheduled email reminders, respects daily quotas, ensures idempotency
- **PostgreSQL**: Stores users, tasks, attachments, audit logs, refresh tokens
- **Mailgun**: Delivers transactional emails (reminders, notifications)

---

### Level 2: Container Diagram

```mermaid
graph TB
    User[👤 Users]
    
    subgraph Docker_Network["Docker Network: tasktracker-network"]
        subgraph UI_Container["UI Container (Nginx)"]
            React["React 18 + TypeScript<br/>━━━━━━━━━━━━━━━<br/>• Vite 5.0 build tool<br/>• Tailwind CSS styling<br/>• Axios HTTP client<br/>• React Router v6<br/>• Hot Toast notifications"]
            Nginx["Nginx Alpine<br/>━━━━━━━━━━━━━━━<br/>• Serve static files<br/>• SPA routing (try_files)<br/>• Gzip compression"]
        end
        
        subgraph API_Container["API Container (.NET 9)"]
            Controllers["Controllers<br/>━━━━━━━━━━━━━━━<br/>• TasksController<br/>• AuthController<br/>• AttachmentsController<br/>• AuditLogsController"]
            Middleware["Middleware Pipeline<br/>━━━━━━━━━━━━━━━<br/>• Exception Handler<br/>• Serilog Logging<br/>• CORS<br/>• Rate Limiting<br/>• JWT Authentication<br/>• Authorization"]
            Services["Service Layer<br/>━━━━━━━━━━━━━━━<br/>• TaskService<br/>• AuthService<br/>• AttachmentService<br/>• AuditLogService"]
            Repositories["Repository Layer<br/>━━━━━━━━━━━━━━━<br/>• TaskRepository<br/>• UserRepository<br/>• AttachmentRepository<br/>• AuditLogRepository"]
            EFCore["EF Core 9<br/>━━━━━━━━━━━━━━━<br/>• DbContext<br/>• Migrations<br/>• Change Tracking<br/>• Connection Pooling"]
        end
        
        subgraph Worker_Container["Worker Container (.NET 9)"]
            HostedService["ReminderHostedService<br/>━━━━━━━━━━━━━━━<br/>• Timer-based execution<br/>• 30-60 min intervals<br/>• Health tracking"]
            ReminderService["ReminderService<br/>━━━━━━━━━━━━━━━<br/>• Query due tasks<br/>• Check idempotency<br/>• Enforce quota"]
            MailgunService["MailgunEmailService<br/>━━━━━━━━━━━━━━━<br/>• REST HTTP client<br/>• HTML email templates<br/>• Error handling"]
        end
        
        subgraph DB_Container["PostgreSQL Container"]
            PostgreSQL[("PostgreSQL 16 Alpine<br/>━━━━━━━━━━━━━━━<br/>• JSONB support (Tags)<br/>• UUID primary keys<br/>• GIN indexes<br/>• Connection pooling<br/>• Health checks")]
        end
    end
    
    Mailgun["📧 Mailgun API<br/>(api.mailgun.net/v3)"]
    
    User -->|HTTPS<br/>Port 3000| Nginx
    Nginx --> React
    React -->|REST API<br/>JSON| Middleware
    Middleware --> Controllers
    Controllers --> Services
    Services --> Repositories
    Repositories --> EFCore
    EFCore -->|Npgsql Driver<br/>Port 5432| PostgreSQL
    
    HostedService -->|Timer| ReminderService
    ReminderService --> Repositories
    ReminderService -->|Send Email| MailgunService
    MailgunService -->|HTTPS POST| Mailgun
    
    style React fill:#61dafb,stroke:#333,stroke-width:2px,color:#000
    style Nginx fill:#009639,stroke:#333,stroke-width:2px,color:#fff
    style Controllers fill:#512bd4,stroke:#333,stroke-width:2px,color:#fff
    style Services fill:#8e44ad,stroke:#333,stroke-width:2px,color:#fff
    style Repositories fill:#d35400,stroke:#333,stroke-width:2px,color:#fff
    style EFCore fill:#e67e22,stroke:#333,stroke-width:2px,color:#fff
    style PostgreSQL fill:#336791,stroke:#333,stroke-width:3px,color:#fff
    style HostedService fill:#512bd4,stroke:#333,stroke-width:2px,color:#fff
    style ReminderService fill:#8e44ad,stroke:#333,stroke-width:2px,color:#fff
    style MailgunService fill:#f06529,stroke:#333,stroke-width:2px,color:#fff
```

**Container Resources:**

| Container | Base Image | CPU Limit | Memory Limit | Restart Policy | Volumes |
|-----------|-----------|-----------|--------------|----------------|---------|
| UI | node:18-alpine → nginx:alpine | 0.5 core | 512 MB | unless-stopped | None (stateless) |
| API | mcr.microsoft.com/dotnet/aspnet:9.0 | 1.0 core | 1 GB | unless-stopped | api_logs |
| Worker | mcr.microsoft.com/dotnet/aspnet:9.0 | 0.5 core | 512 MB | unless-stopped | worker_logs |
| PostgreSQL | postgres:16-alpine | 1.0 core | 1 GB | unless-stopped | postgres_data |

---

### Level 3: Clean Architecture Layers

```mermaid
graph TD
    subgraph API_Layer["🌐 API Layer (Presentation)"]
        direction TB
        Controllers_API["Controllers<br/>━━━━━━━━━━━━━━━<br/>TasksController<br/>AuthController<br/>AttachmentsController<br/>AuditLogsController"]
        Middleware_API["Middleware<br/>━━━━━━━━━━━━━━━<br/>GlobalExceptionHandler<br/>SerilogRequestLogging<br/>CorsMiddleware<br/>RateLimiterMiddleware"]
        Program["Program.cs<br/>━━━━━━━━━━━━━━━<br/>DI Configuration<br/>Middleware Pipeline<br/>Swagger Setup"]
    end
    
    subgraph Application_Layer["💼 Application Layer (Business Logic)"]
        direction TB
        Services_App["Services<br/>━━━━━━━━━━━━━━━<br/>TaskService<br/>AuthService<br/>AttachmentService<br/>AuditLogService<br/>CurrentUserService"]
        DTOs_App["DTOs<br/>━━━━━━━━━━━━━━━<br/>TaskDto, CreateTaskDto<br/>UpdateTaskDto<br/>PaginatedResponse<T>"]
        Validators_App["Validators<br/>━━━━━━━━━━━━━━━<br/>CreateTaskValidator<br/>UpdateTaskValidator<br/>RegisterValidator<br/>FluentValidation Rules"]
        Interfaces_App["Interfaces<br/>━━━━━━━━━━━━━━━<br/>ITaskService<br/>ITaskRepository<br/>IEmailService<br/>ICurrentUserService"]
    end
    
    subgraph Infrastructure_Layer["🔧 Infrastructure Layer (Data & External)"]
        direction TB
        Repositories_Infra["Repositories<br/>━━━━━━━━━━━━━━━<br/>TaskRepository<br/>UserRepository<br/>AttachmentRepository<br/>AuditLogRepository<br/>RefreshTokenRepository"]
        DbContext_Infra["EF Core<br/>━━━━━━━━━━━━━━━<br/>ApplicationDbContext<br/>Migrations<br/>Interceptors (Audit)<br/>Configurations"]
        External_Infra["External Services<br/>━━━━━━━━━━━━━━━<br/>MailgunEmailService<br/>FileStorageService<br/>TokenService"]
    end
    
    subgraph Domain_Layer["🎯 Domain Layer (Core Business)"]
        direction TB
        Entities_Domain["Entities<br/>━━━━━━━━━━━━━━━<br/>TaskItem<br/>User<br/>Attachment<br/>AuditLog<br/>RefreshToken"]
        Enums_Domain["Enums<br/>━━━━━━━━━━━━━━━<br/>TaskStatus<br/>Priority<br/>AuditAction"]
        Common_Domain["Common<br/>━━━━━━━━━━━━━━━<br/>BaseEntity<br/>IAuditableEntity<br/>ISoftDeletable"]
    end
    
    Database[("💾 PostgreSQL<br/>Database")]
    FileSystem["📁 File System<br/>/Uploads"]
    EmailAPI["📧 Mailgun API"]
    
    Controllers_API --> Middleware_API
    Middleware_API --> Services_App
    Services_App --> DTOs_App
    Services_App --> Validators_App
    Services_App --> Interfaces_App
    Repositories_Infra -.implements.-> Interfaces_App
    Repositories_Infra --> DbContext_Infra
    External_Infra -.implements.-> Interfaces_App
    DbContext_Infra --> Entities_Domain
    Entities_Domain --> Common_Domain
    Entities_Domain --> Enums_Domain
    DbContext_Infra --> Database
    External_Infra --> FileSystem
    External_Infra --> EmailAPI
    
    style API_Layer fill:#e3f2fd,stroke:#1976d2,stroke-width:3px
    style Application_Layer fill:#f3e5f5,stroke:#7b1fa2,stroke-width:3px
    style Infrastructure_Layer fill:#fff3e0,stroke:#f57c00,stroke-width:3px
    style Domain_Layer fill:#e8f5e9,stroke:#388e3c,stroke-width:3px
    style Database fill:#336791,stroke:#333,stroke-width:2px,color:#fff
```

**Dependency Rules:**

```
┌─────────────────────────────────────────────────────┐
│ Dependency Direction (Inward Only)                  │
├─────────────────────────────────────────────────────┤
│ API Layer         ──►  Application Layer            │
│ Application Layer ──►  Domain Layer                 │
│ Infrastructure    ──►  Application Layer (via DI)   │
│                                                      │
│ ⛔ Domain Layer has ZERO external dependencies      │
│ ✅ Infrastructure implements Application interfaces │
└─────────────────────────────────────────────────────┘
```

---

## 🎨 UML Diagrams

### Class Diagram - Domain Model

```mermaid
classDiagram
    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +bool IsDeleted
    }
    
    class User {
        +string Email
        +string PasswordHash
        +string FirstName
        +string LastName
        +ICollection~TaskItem~ Tasks
        +ICollection~RefreshToken~ RefreshTokens
    }
    
    class TaskItem {
        +Guid UserId
        +string Title
        +string Description
        +TaskStatus Status
        +Priority Priority
        +DateTime? DueDate
        +List~string~ Tags
        +User User
        +ICollection~Attachment~ Attachments
        +ICollection~AuditLog~ AuditLogs
    }
    
    class Attachment {
        +Guid TaskId
        +Guid UploadedByUserId
        +string FileName
        +string FilePath
        +long FileSize
        +string ContentType
        +DateTime UploadedAt
        +TaskItem Task
        +User UploadedBy
    }
    
    class AuditLog {
        +string EntityType
        +string EntityId
        +string Action
        +string? Details
        +DateTime Timestamp
        +Guid? UserId
        +Guid? TaskId
        +User? User
        +TaskItem? Task
    }
    
    class RefreshToken {
        +Guid UserId
        +string Token
        +DateTime ExpiresAt
        +DateTime? RevokedAt
        +bool IsRevoked
        +User User
    }
    
    class TaskStatus {
        <<enumeration>>
        Pending
        InProgress
        Completed
        Cancelled
    }
    
    class Priority {
        <<enumeration>>
        Low
        Medium
        High
        Critical
    }
    
    BaseEntity <|-- User
    BaseEntity <|-- TaskItem
    BaseEntity <|-- Attachment
    BaseEntity <|-- AuditLog
    BaseEntity <|-- RefreshToken
    
    User "1" --> "*" TaskItem : owns
    User "1" --> "*" RefreshToken : has
    User "1" --> "*" Attachment : uploads
    TaskItem "1" --> "*" Attachment : has
    TaskItem "1" --> "*" AuditLog : tracked by
    User "1" --> "*" AuditLog : performs
    TaskItem ..> TaskStatus : uses
    TaskItem ..> Priority : uses
```

### Class Diagram - Service Layer

```mermaid
classDiagram
    class ITaskService {
        <<interface>>
        +GetTasksAsync(filter) Task~PaginatedResponse~TaskDto~~
        +GetTaskByIdAsync(id) Task~TaskDto~
        +CreateTaskAsync(dto) Task~TaskDto~
        +UpdateTaskAsync(id, dto) Task~TaskDto~
        +DeleteTaskAsync(id) Task~bool~
    }
    
    class TaskService {
        -ITaskRepository _taskRepository
        -ICurrentUserService _currentUserService
        -IAuditLogRepository _auditLogRepository
        +GetTasksAsync(filter)
        +GetTaskByIdAsync(id)
        +CreateTaskAsync(dto)
        +UpdateTaskAsync(id, dto)
        +DeleteTaskAsync(id)
    }
    
    class ITaskRepository {
        <<interface>>
        +GetByIdAsync(id) Task~TaskItem~
        +GetFilteredTasksAsync(filter) Task~PaginatedResponse~TaskItem~~
        +AddAsync(task) Task~TaskItem~
        +UpdateAsync(task) Task
        +DeleteAsync(id) Task~bool~
    }
    
    class TaskRepository {
        -ApplicationDbContext _context
        +GetByIdAsync(id)
        +GetFilteredTasksAsync(filter)
        +AddAsync(task)
        +UpdateAsync(task)
        +DeleteAsync(id)
    }
    
    class ICurrentUserService {
        <<interface>>
        +UserId Guid?
        +Email string?
        +IsAuthenticated bool
    }
    
    class CurrentUserService {
        -IHttpContextAccessor _httpContextAccessor
        +UserId Guid?
        +Email string?
        +IsAuthenticated bool
        -GetClaimValue(type) string?
    }
    
    ITaskService <|.. TaskService : implements
    ITaskRepository <|.. TaskRepository : implements
    ICurrentUserService <|.. CurrentUserService : implements
    TaskService --> ITaskRepository : uses
    TaskService --> ICurrentUserService : uses
    TaskService --> IAuditLogRepository : uses
```

---

## 🗄️ Database Architecture

### Entity-Relationship Diagram

```mermaid
erDiagram
    USERS ||--o{ TASKS : owns
    USERS ||--o{ REFRESH_TOKENS : has
    USERS ||--o{ ATTACHMENTS : uploads
    USERS ||--o{ AUDIT_LOGS : performs
    TASKS ||--o{ ATTACHMENTS : has
    TASKS ||--o{ AUDIT_LOGS : tracked_by
    
    USERS {
        uuid Id PK
        string Email UK "UNIQUE, NOT NULL"
        string PasswordHash "NOT NULL"
        string FirstName "NOT NULL"
        string LastName "NOT NULL"
        timestamp CreatedAt "NOT NULL"
        timestamp UpdatedAt "NOT NULL"
        boolean IsDeleted "DEFAULT false"
    }
    
    TASKS {
        uuid Id PK
        uuid UserId FK "NOT NULL"
        string Title "NOT NULL, MAX 200"
        string Description "MAX 2000"
        int Status "NOT NULL, 1-4"
        int Priority "NOT NULL, 1-4"
        timestamp DueDate "NULLABLE"
        jsonb Tags "ARRAY, DEFAULT []"
        timestamp CreatedAt "NOT NULL"
        timestamp UpdatedAt "NOT NULL"
        boolean IsDeleted "DEFAULT false"
    }
    
    ATTACHMENTS {
        uuid Id PK
        uuid TaskId FK "NOT NULL"
        uuid UploadedByUserId FK "NOT NULL"
        string FileName "NOT NULL"
        string FilePath "NOT NULL"
        bigint FileSize "NOT NULL"
        string ContentType "NOT NULL"
        timestamp UploadedAt "NOT NULL"
        timestamp CreatedAt "NOT NULL"
        timestamp UpdatedAt "NOT NULL"
        boolean IsDeleted "DEFAULT false"
    }
    
    AUDIT_LOGS {
        uuid Id PK
        string EntityType "NOT NULL"
        string EntityId "NOT NULL"
        string Action "NOT NULL"
        string Details "NULLABLE"
        timestamp Timestamp "NOT NULL"
        uuid UserId FK "NULLABLE"
        uuid TaskId FK "NULLABLE"
    }
    
    REFRESH_TOKENS {
        uuid Id PK
        uuid UserId FK "NOT NULL"
        string Token UK "UNIQUE, NOT NULL"
        timestamp ExpiresAt "NOT NULL"
        timestamp CreatedAt "NOT NULL"
        timestamp RevokedAt "NULLABLE"
        boolean IsRevoked "DEFAULT false"
    }
```

### Database Indexes

| Table | Index Name | Columns | Type | Purpose |
|-------|-----------|---------|------|---------|
| Users | IX_Users_Email | Email | BTREE | Fast email lookup for login |
| Tasks | IX_Tasks_UserId | UserId | BTREE | Filter tasks by owner |
| Tasks | IX_Tasks_Status | Status | BTREE | Filter by status |
| Tasks | IX_Tasks_DueDate | DueDate | BTREE | Find tasks due soon |
| Tasks | IX_Tasks_Tags | Tags | GIN | JSONB array search |
| Attachments | IX_Attachments_TaskId | TaskId | BTREE | Load task attachments |
| AuditLogs | IX_AuditLogs_EntityType | EntityType | BTREE | Filter logs by entity |
| AuditLogs | IX_AuditLogs_TaskId | TaskId | BTREE | Task audit history |
| RefreshTokens | IX_RefreshTokens_Token | Token | BTREE | Validate refresh tokens |
| RefreshTokens | IX_RefreshTokens_UserId | UserId | BTREE | User token management |

---

## 🔄 Sequence Diagrams

### User Authentication Flow

```mermaid
sequenceDiagram
    actor User
    participant UI as React UI
    participant API as API Controller
    participant Auth as AuthService
    participant UserRepo as UserRepository
    participant Token as TokenService
    participant DB as PostgreSQL
    
    User->>UI: Enter email & password
    UI->>API: POST /api/auth/login
    API->>Auth: LoginAsync(email, password)
    Auth->>UserRepo: GetByEmailAsync(email)
    UserRepo->>DB: SELECT * FROM Users WHERE Email = ?
    DB-->>UserRepo: User entity
    UserRepo-->>Auth: User
    
    Auth->>Auth: BCrypt.Verify(password, hash)
    alt Password Valid
        Auth->>Token: GenerateAccessToken(user)
        Token-->>Auth: JWT (60 min expiry)
        Auth->>Token: GenerateRefreshToken()
        Token-->>Auth: Refresh Token (7 days)
        Auth->>DB: INSERT INTO RefreshTokens
        Auth-->>API: LoginResponse (tokens + user)
        API-->>UI: 200 OK + JSON
        UI->>UI: Store tokens in localStorage
        UI-->>User: Redirect to /tasks
    else Password Invalid
        Auth-->>API: Unauthorized
        API-->>UI: 401 Unauthorized
        UI-->>User: Show error message
    end
```

### Task Creation Flow

```mermaid
sequenceDiagram
    actor User
    participant UI as React UI
    participant API as TasksController
    participant Service as TaskService
    participant Validator as FluentValidator
    participant Repo as TaskRepository
    participant Audit as AuditLogRepository
    participant DB as PostgreSQL
    
    User->>UI: Fill task form & submit
    UI->>API: POST /api/tasks + JWT
    API->>API: Validate JWT
    API->>Validator: Validate(CreateTaskDto)
    
    alt Validation Fails
        Validator-->>API: ValidationException
        API-->>UI: 400 Bad Request + errors
        UI-->>User: Show validation errors
    else Validation Passes
        Validator-->>API: Valid
        API->>Service: CreateTaskAsync(dto)
        Service->>Service: Get UserId from JWT
        Service->>Repo: AddAsync(taskEntity)
        Repo->>DB: INSERT INTO Tasks
        DB-->>Repo: Task with Id
        Repo-->>Service: TaskItem
        
        Service->>Audit: Create audit log
        Audit->>DB: INSERT INTO AuditLogs
        
        Service-->>API: TaskDto
        API-->>UI: 201 Created + TaskDto
        UI->>UI: Add task to state
        UI-->>User: Show success toast
    end
```

### Email Reminder Flow

```mermaid
sequenceDiagram
    participant Timer as Hosted Service Timer
    participant Reminder as ReminderService
    participant TaskRepo as TaskRepository
    participant AuditRepo as AuditLogRepository
    participant Email as MailgunEmailService
    participant Mailgun as Mailgun API
    participant DB as PostgreSQL
    
    Timer->>Reminder: ExecuteAsync() (every 60 min)
    Reminder->>TaskRepo: Get tasks due in 24h
    TaskRepo->>DB: SELECT * FROM Tasks<br/>WHERE DueDate BETWEEN NOW() AND NOW() + 24h<br/>AND Status != Completed<br/>AND IsDeleted = false
    DB-->>TaskRepo: List<TaskItem>
    TaskRepo-->>Reminder: Tasks (sorted by DueDate, Priority)
    
    loop For each task (max 50)
        Reminder->>AuditRepo: Check if reminder sent
        AuditRepo->>DB: SELECT * FROM AuditLogs<br/>WHERE TaskId = ? AND Action = 'ReminderSent'
        DB-->>AuditRepo: AuditLog or null
        
        alt Reminder already sent
            AuditRepo-->>Reminder: Exists
            Reminder->>Reminder: Skip (idempotency)
        else Not sent
            AuditRepo-->>Reminder: Not found
            Reminder->>Reminder: Check daily quota (90/day)
            
            alt Quota available
                Reminder->>Email: SendReminderEmailAsync(task, user)
                Email->>Mailgun: POST /messages<br/>HTML email + attachments
                Mailgun-->>Email: 200 OK + message ID
                Email-->>Reminder: Success
                
                Reminder->>AuditRepo: Create audit log
                AuditRepo->>DB: INSERT INTO AuditLogs<br/>(Action='ReminderSent')
                Reminder->>Reminder: Increment counter
            else Quota exceeded
                Reminder->>Reminder: Stop processing
            end
        end
    end
    
    Reminder-->>Timer: Complete (wait 60 min)
```

---

## 🚀 Deployment Architecture

### Docker Compose Orchestration

```mermaid
graph TB
    subgraph Host["Docker Desktop Host Machine"]
        subgraph Network["tasktracker-network (Bridge)"]
            subgraph UI_Service["UI Service"]
                UI_Container["tasktracker-ui<br/>━━━━━━━━━━━━━━━<br/>Image: Custom (Nginx)<br/>Port: 3000:80<br/>Restart: unless-stopped"]
            end
            
            subgraph API_Service["API Service"]
                API_Container["tasktracker-api<br/>━━━━━━━━━━━━━━━<br/>Image: Custom (.NET 9)<br/>Port: 5128:5128<br/>Restart: unless-stopped<br/>Health: /health (30s)<br/>Depends: postgres (healthy)"]
            end
            
            subgraph Worker_Service["Worker Service"]
                Worker_Container["tasktracker-worker<br/>━━━━━━━━━━━━━━━<br/>Image: Custom (.NET 9)<br/>Port: 5129:5129<br/>Restart: unless-stopped<br/>Health: /health (60s)<br/>Depends: api + postgres (healthy)"]
            end
            
            subgraph DB_Service["Database Service"]
                DB_Container["tasktracker-postgres<br/>━━━━━━━━━━━━━━━<br/>Image: postgres:16-alpine<br/>Port: 5433:5432<br/>Restart: unless-stopped<br/>Health: pg_isready (10s)"]
            end
        end
        
        subgraph Volumes["Docker Volumes (Persistent)"]
            PG_Data["postgres_data<br/>━━━━━━━━━━━━━━━<br/>Database files<br/>~500MB"]
            API_Logs["api_logs<br/>━━━━━━━━━━━━━━━<br/>API log files<br/>~100MB"]
            Worker_Logs["worker_logs<br/>━━━━━━━━━━━━━━━<br/>Worker log files<br/>~50MB"]
        end
    end
    
    External["📧 Mailgun API<br/>(External)"]
    
    UI_Container -.->|Port Mapping| Host
    API_Container -.->|Port Mapping| Host
    Worker_Container -.->|Port Mapping| Host
    DB_Container -.->|Port Mapping| Host
    
    DB_Container --> PG_Data
    API_Container --> API_Logs
    Worker_Container --> Worker_Logs
    
    UI_Container -->|HTTP| API_Container
    API_Container -->|SQL| DB_Container
    Worker_Container -->|SQL| DB_Container
    Worker_Container -->|HTTPS| External
    
    style UI_Container fill:#61dafb,stroke:#333,stroke-width:2px,color:#000
    style API_Container fill:#512bd4,stroke:#333,stroke-width:2px,color:#fff
    style Worker_Container fill:#512bd4,stroke:#333,stroke-width:2px,color:#fff
    style DB_Container fill:#336791,stroke:#333,stroke-width:2px,color:#fff
    style PG_Data fill:#ffe0b2,stroke:#f57c00,stroke-width:2px
    style API_Logs fill:#ffe0b2,stroke:#f57c00,stroke-width:2px
    style Worker_Logs fill:#ffe0b2,stroke:#f57c00,stroke-width:2px
```

### Health Check Configuration

| Service | Endpoint | Interval | Timeout | Retries | Start Period |
|---------|----------|----------|---------|---------|--------------|
| PostgreSQL | `pg_isready` | 10s | 5s | 5 | N/A |
| API | `GET /health` | 30s | 10s | 3 | 40s |
| Worker | `GET /health` | 60s | 10s | 3 | 30s |
| UI | N/A (static) | N/A | N/A | N/A | N/A |

### Startup Sequence

```
1. PostgreSQL starts → waits for health check (pg_isready)
2. API starts → depends on PostgreSQL (healthy) → waits for own health check
3. Worker starts → depends on API + PostgreSQL (both healthy) → waits for health check
4. UI starts → depends on API (healthy) → no health check (static files)
```

### Deployment Diagram (Docker Desktop)

```
┌────────────────────────────────────────────────────────────────────┐
│                       Docker Desktop Host                          │
│                                                                    │
│  ┌──────────────────────┐     ┌──────────────────────┐           │
│  │  task-tracker-ui     │     │  task-tracker-api    │           │
│  │  ──────────────────  │     │  ──────────────────  │           │
│  │  Container           │     │  Container           │           │
│  │  ────────           │     │  ────────           │           │
│  │  • Node.js 18        │     │  • .NET 9 Runtime    │           │
│  │  • Vite Dev Server   │     │  • Kestrel HTTP      │           │
│  │  • Port: 3000        │     │  • Port: 5128        │           │
│  │  • Volume: /app      │     │  • Volume: logs/     │           │
│  └──────────────────────┘     └──────────┬───────────┘           │
│                                           │                        │
│  ┌──────────────────────┐     ┌──────────▼───────────┐           │
│  │ task-tracker-worker  │     │ tasktracker-postgres │           │
│  │ ──────────────────── │     │ ──────────────────── │           │
│  │  Container           │     │  Container           │           │
│  │  ────────           │     │  ────────           │           │
│  │  • .NET 9 Runtime    │     │  • PostgreSQL 16     │           │
│  │  • Worker Service    │     │  • Port: 5433        │           │
│  │  • Volume: logs/     │     │  • Volume: data/     │           │
│  └──────────┬───────────┘     └──────────────────────┘           │
│             │                                                     │
│             └────────────► PostgreSQL ◄────────────┘             │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │              Docker Network: tasktracker-network             │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│  External Volumes:                                                │
│  • postgres_data (persistent database storage)                   │
│  • api_logs (API application logs)                               │
│  • worker_logs (Worker service logs)                             │
└────────────────────────────────────────────────────────────────────┘
```

### 6. Data Flow Diagrams

#### Task Creation Flow

```
User ──► React UI ──► POST /api/tasks ──► TasksController
                                             │
                                             ▼
                                        TaskService
                                             │
                                             ▼
                                        TaskRepository
                                             │
                                             ▼
                                        EF Core DbContext
                                             │
                                             ▼
                                        PostgreSQL
                                             │
                                             ▼
                                        AuditLog Created
                                             │
                                             ▼
                                        Metrics Recorded
```

#### Email Reminder Flow

```
Worker Service (every 30-60 min)
    │
    ▼
ReminderHostedService
    │
    ▼
ReminderService.ProcessRemindersAsync()
    │
    ├──► Query Tasks (due within 24h, not completed)
    │
    ├──► Check AuditLog (idempotency - already sent?)
    │
    ├──► Check Daily Quota (90 emails/day)
    │
    ├──► Sort by DueDate, Priority
    │
    └──► For each task:
         ├──► MailgunEmailService.SendReminderEmailAsync()
         │       │
         │       ▼
         │    Mailgun API (HTTP POST)
         │
         ├──► AuditLog.Create("Reminder sent")
         │
         └──► Metrics.RecordReminderSent()
```

#### Authentication Flow

```
User Login Request
    │
    ▼
POST /api/auth/login
    │
    ▼
AuthController.Login()
    │
    ▼
AuthService.LoginAsync()
    │
    ├──► UserRepository.GetByEmailAsync()
    │
    ├──► BCrypt.Verify(password, hash)
    │
    ├──► TokenService.GenerateAccessToken() (JWT, 60 min)
    │
    ├──► TokenService.GenerateRefreshToken() (7 days)
    │
    ├──► RefreshTokenRepository.SaveAsync()
    │
    └──► Return LoginResponseDto
         {
           accessToken: "eyJ...",
           refreshToken: "...",
           user: {...}
         }
```

---

## 🔒 Security Architecture

### Authentication & Authorization Flow

```mermaid
graph TB
    subgraph Client["Client (Browser)"]
        LocalStorage["localStorage<br/>━━━━━━━━━━━━━━━<br/>• accessToken (JWT)<br/>• refreshToken<br/>• user info"]
    end
    
    subgraph API_Security["API Security Layers"]
        HTTPS["1️⃣ HTTPS/TLS<br/>━━━━━━━━━━━━━━━<br/>Transport encryption<br/>TLS 1.2+"]
        
        RateLimit["2️⃣ Rate Limiting<br/>━━━━━━━━━━━━━━━<br/>• Per-User: 100/min<br/>• Per-IP Auth: 20/15min<br/>• Per-IP Strict: 10/min"]
        
        JWT_Auth["3️⃣ JWT Authentication<br/>━━━━━━━━━━━━━━━<br/>• Validate signature (HS256)<br/>• Check expiration<br/>• Extract claims"]
        
        Authorization["4️⃣ Authorization<br/>━━━━━━━━━━━━━━━<br/>• [Authorize] attribute<br/>• Owner-based checks<br/>• CurrentUserService"]
        
        Validation["5️⃣ Input Validation<br/>━━━━━━━━━━━━━━━<br/>• FluentValidation<br/>• Model State<br/>• File size limits"]
        
        AuditLogging["6️⃣ Audit Logging<br/>━━━━━━━━━━━━━━━<br/>• Track all actions<br/>• User attribution<br/>• Timestamp"]
    end
    
    subgraph Database_Security["Database Security"]
        PasswordHash["BCrypt Hashing<br/>━━━━━━━━━━━━━━━<br/>• Cost factor: 11<br/>• Unique salt per password<br/>• 100ms computation time"]
        
        RefreshTokenDB["Refresh Tokens<br/>━━━━━━━━━━━━━━━<br/>• Stored in DB<br/>• Can be revoked<br/>• 7-day expiry"]
        
        Parameterized["Parameterized Queries<br/>━━━━━━━━━━━━━━━<br/>• EF Core prevents SQL injection<br/>• No raw SQL concatenation"]
    end
    
    LocalStorage --> HTTPS
    HTTPS --> RateLimit
    RateLimit --> JWT_Auth
    JWT_Auth --> Authorization
    Authorization --> Validation
    Validation --> AuditLogging
    JWT_Auth -.verifies.-> RefreshTokenDB
    Authorization -.validates against.-> PasswordHash
    Validation -.safe queries.-> Parameterized
    
    style HTTPS fill:#e8f5e9,stroke:#388e3c,stroke-width:2px
    style RateLimit fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    style JWT_Auth fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    style Authorization fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    style Validation fill:#fce4ec,stroke:#c2185b,stroke-width:2px
    style AuditLogging fill:#e0f2f1,stroke:#00897b,stroke-width:2px
```

### Security Measures Summary

| Layer | Implementation | Protection Against |
|-------|---------------|-------------------|
| **Transport** | HTTPS (TLS 1.2+) | Man-in-the-middle attacks, eavesdropping |
| **Rate Limiting** | 3-tier ASP.NET Core policy | Brute force, DDoS, credential stuffing |
| **Authentication** | JWT (HS256) + BCrypt | Unauthorized access, password guessing |
| **Authorization** | [Authorize] + ownership checks | Privilege escalation, data access violations |
| **Input Validation** | FluentValidation + model state | Injection attacks, malformed data |
| **Audit Logging** | All operations logged | Forensics, compliance, accountability |
| **CORS** | Specific origin whitelist | Cross-origin attacks |
| **SQL Injection** | EF Core parameterized queries | Database manipulation |

### Data Flow Diagrams

#### Task Creation Flow

```
User ──► React UI ──► POST /api/tasks ──► TasksController
                                             │
                                             ▼
                                        TaskService
                                             │
                                             ▼
                                        TaskRepository
                                             │
                                             ▼
                                        EF Core DbContext
                                             │
                                             ▼
                                        PostgreSQL
                                             │
                                             ▼
                                        AuditLog Created
                                             │
                                             ▼
                                        Metrics Recorded
```

#### Authentication Flow

```
User Login Request
    │
    ▼
POST /api/auth/login
    │
    ▼
AuthController.Login()
    │
    ▼
AuthService.LoginAsync()
    │
    ├──► UserRepository.GetByEmailAsync()
    │
    ├──► BCrypt.Verify(password, hash)
    │
    ├──► TokenService.GenerateAccessToken() (JWT, 60 min)
    │
    ├──► TokenService.GenerateRefreshToken() (7 days)
    │
    ├──► RefreshTokenRepository.SaveAsync()
    │
    └──► Return LoginResponseDto
         {
           accessToken: "eyJ...",
           refreshToken: "...",
           user: {...}
         }
```

---

## 📈 Monitoring & Observability

### Metrics Architecture

```mermaid
graph LR
    subgraph Services["TaskTracker Services"]
        API[API Service<br/>Port 5128]
        Worker[Worker Service<br/>Port 5129]
    end
    
    subgraph Prometheus_Stack["Observability Stack (Optional)"]
        Prometheus[Prometheus<br/>Metrics Collector]
        Grafana[Grafana<br/>Dashboards]
        AlertManager[Alert Manager<br/>Notifications]
    end
    
    subgraph Logs["Log Storage"]
        ConsoleLogs[Console Output<br/>Docker Logs]
        FileLogs[File Logs<br/>api_logs / worker_logs]
    end
    
    API -->|/metrics<br/>Prometheus format| Prometheus
    Worker -->|/metrics<br/>Prometheus format| Prometheus
    Prometheus --> Grafana
    Prometheus --> AlertManager
    
    API --> ConsoleLogs
    API --> FileLogs
    Worker --> ConsoleLogs
    Worker --> FileLogs
    
    style API fill:#512bd4,stroke:#333,stroke-width:2px,color:#fff
    style Worker fill:#512bd4,stroke:#333,stroke-width:2px,color:#fff
    style Prometheus fill:#e6522c,stroke:#333,stroke-width:2px,color:#fff
    style Grafana fill:#f46800,stroke:#333,stroke-width:2px,color:#fff
```

### Key Metrics Exposed

| Metric Type | Metric Name | Description |
|-------------|------------|-------------|
| **HTTP** | `http_requests_received_total` | Total HTTP requests by method, endpoint, status |
| **HTTP** | `http_request_duration_seconds` | Request duration histogram |
| **Tasks** | `tasks_created_total` | Counter of tasks created |
| **Tasks** | `tasks_updated_total` | Counter of tasks updated |
| **Tasks** | `tasks_deleted_total` | Counter of tasks deleted |
| **Auth** | `auth_login_attempts_total` | Login attempts (success/failure) |
| **Email** | `worker_reminders_sent_total` | Email reminders sent |
| **Email** | `worker_email_quota_remaining` | Daily email quota remaining |
| **Health** | `worker_last_run_timestamp` | Last successful worker execution |

### Logging Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-DD HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/api-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Health Checks

**API Health Endpoints:**
- `GET /health` - Overall status with JSON details (database connectivity, memory usage, response time)
- `GET /health/db` - Database connectivity only

**Worker Health Service:**
- Last successful run timestamp
- Failed jobs count
- Total jobs run
- Health status (healthy if run within 2 hours)

### Observability Stack Details

```
┌─────────────────────────────────────────────────────────────────┐
│                   Observability Stack                            │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                      Metrics                              │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │ Prometheus Metrics                                  │  │  │
│  │  │ ──────────────────                                 │  │  │
│  │  │ • HTTP request count, duration, errors             │  │  │
│  │  │ • Task operations (created, updated, deleted)      │  │  │
│  │  │ • Authentication success/failure                    │  │  │
│  │  │ • Worker reminders (sent, failed, skipped)         │  │  │
│  │  │ • Email quota remaining                             │  │  │
│  │  │                                                     │  │  │
│  │  │ Endpoints:                                          │  │  │
│  │  │ • GET /metrics (API)                                │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                      Logging                              │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │ Serilog (Structured Logging)                        │  │  │
│  │  │ ────────────────────────────                       │  │  │
│  │  │ Sinks:                                              │  │  │
│  │  │ • Console (stdout)                                  │  │  │
│  │  │ • File (rolling daily)                              │  │  │
│  │  │   - logs/api-YYYYMMDD.log                          │  │  │
│  │  │   - logs/worker-YYYYMMDD.log                       │  │  │
│  │  │                                                     │  │  │
│  │  │ Log Levels:                                         │  │  │
│  │  │ • Production: Information, Warning, Error           │  │  │
│  │  │ • Development: Debug, Information                   │  │  │
│  │  │                                                     │  │  │
│  │  │ Enrichers:                                          │  │  │
│  │  │ • Correlation ID (request tracking)                 │  │  │
│  │  │ • Timestamp, Level, Message, Exception              │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                   Health Checks                           │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │ API Health Endpoints                                │  │  │
│  │  │ ────────────────────                               │  │  │
│  │  │ • GET /health (overall status with JSON details)   │  │  │
│  │  │   - Database connectivity                           │  │  │
│  │  │   - Memory usage                                    │  │  │
│  │  │   - Response time                                   │  │  │
│  │  │                                                     │  │  │
│  │  │ • GET /health/db (database only)                    │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  │                                                            │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │ Worker Health Service                               │  │  │
│  │  │ ─────────────────────                              │  │  │
│  │  │ • Last successful run timestamp                     │  │  │
│  │  │ • Failed jobs count                                 │  │  │
│  │  │ • Total jobs run                                    │  │  │
│  │  │ • Health status (healthy if run within 2 hours)    │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 Technology Stack

### Backend Technologies

| Category | Technology | Version | Purpose |
|----------|-----------|---------|---------|
| **Framework** | .NET | 9.0 | Web API + Worker Service |
| **Web Framework** | ASP.NET Core | 9.0 | REST API, Middleware, DI |
| **ORM** | Entity Framework Core | 9.0 | Database access, migrations |
| **Database Driver** | Npgsql | 9.0 | PostgreSQL connectivity |
| **Authentication** | Microsoft.AspNetCore.Authentication.JwtBearer | 9.0 | JWT validation |
| **Password Hashing** | BCrypt.Net-Next | 4.0 | Secure password storage |
| **Validation** | FluentValidation.AspNetCore | 12.0 | Input validation |
| **Logging** | Serilog.AspNetCore | 9.0 | Structured logging |
| **Metrics** | prometheus-net.AspNetCore | 8.2 | Prometheus metrics |
| **API Documentation** | Swashbuckle.AspNetCore | 6.8 | Swagger/OpenAPI |
| **HTTP Client** | RestSharp | 112.0 | Mailgun API calls |
| **Testing** | xUnit + Moq + FluentAssertions | Latest | Unit & integration tests |

### Frontend Technologies

| Category | Technology | Version | Purpose |
|----------|-----------|---------|---------|
| **Framework** | React | 18.2 | UI library |
| **Language** | TypeScript | 5.2 | Type safety |
| **Build Tool** | Vite | 5.0 | Dev server + bundler |
| **Routing** | React Router | 6.20 | Client-side routing |
| **HTTP Client** | Axios | 1.6 | API requests |
| **Styling** | Tailwind CSS | 3.3 | Utility-first CSS |
| **Notifications** | React Hot Toast | 2.4 | Toast messages |
| **Date Utils** | date-fns | 3.0 | Date manipulation |
| **Icons** | Lucide React | 0.294 | Icon library |

### Infrastructure

| Category | Technology | Version | Purpose |
|----------|-----------|---------|---------|
| **Database** | PostgreSQL | 16-alpine | Relational database |
| **Web Server** | Nginx | Alpine | Static file serving |
| **Containerization** | Docker | Latest | Container runtime |
| **Orchestration** | Docker Compose | 3.8 | Multi-container apps |
| **Email Service** | Mailgun | API v3 | Transactional emails |

---

## 📚 Additional Resources

- **[Architecture Rationale](./ARCHITECTURE_RATIONALE.md)** - Detailed design decisions and trade-offs
- **[Database Scripts](../DATABASE_SCRIPTS.md)** - SQL utilities for database management
- **[Docker Deployment Guide](../DOCKER_DEPLOYMENT.md)** - Complete Docker setup instructions
- **[Phase Documentation](../Phases/)** - Implementation phases (1-5)
- **[API Documentation](http://localhost:5128/swagger)** - Interactive OpenAPI docs (when running)

---

**Document Version:** 2.0  
**Status:** Production Ready ✅  
**Last Updated:** November 28, 2025  
**Total Tests:** 113 (passing)  
**Docker Services:** 4 (UI, API, Worker, PostgreSQL)
