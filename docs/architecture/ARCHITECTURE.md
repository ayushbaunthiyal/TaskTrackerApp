# TaskTracker - Architecture Documentation

## System Overview

TaskTracker is a full-stack task management application built using Clean Architecture principles, featuring a .NET 9 backend API, React TypeScript frontend, PostgreSQL database, and a background worker service for automated email reminders.

## Architecture Diagrams

### 1. System Context Diagram (C4 Level 1)

```
┌─────────────────────────────────────────────────────────────────┐
│                         TaskTracker System                       │
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐     │
│  │   Web UI     │    │     API      │    │   Worker     │     │
│  │  (React)     │◄───┤   (.NET 9)   │    │  Service     │     │
│  └──────────────┘    └──────┬───────┘    └──────┬───────┘     │
│                              │                    │              │
│                       ┌──────▼────────────────────▼──────┐      │
│                       │    PostgreSQL Database           │      │
│                       └──────────────────────────────────┘      │
└─────────────────────────────────────────────────────────────────┘
                                   │
                         ┌─────────▼──────────┐
                         │  Mailgun Email API │
                         └────────────────────┘

External Users ──► Web UI (Port 3000)
                    │
                    ▼
                 API (Port 5128)
                    │
                    ▼
              PostgreSQL (Port 5433)
```

### 2. Container Diagram (C4 Level 2)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           TaskTracker Application                            │
│                                                                              │
│  ┌────────────────────┐                                                     │
│  │   React Frontend   │                                                     │
│  │   ────────────     │                                                     │
│  │  • TypeScript      │                                                     │
│  │  • Vite            │                                                     │
│  │  • Tailwind CSS    │                                                     │
│  │  • Axios           │                                                     │
│  └─────────┬──────────┘                                                     │
│            │ HTTP/JSON                                                      │
│            │                                                                │
│  ┌─────────▼──────────┐           ┌──────────────────┐                    │
│  │   .NET 9 Web API   │           │  Worker Service  │                    │
│  │   ──────────────   │           │  ──────────────  │                    │
│  │  • ASP.NET Core    │           │  • Hosted Svc    │                    │
│  │  • JWT Auth        │           │  • Email Queue   │                    │
│  │  • Rate Limiting   │           │  • Scheduled Job │                    │
│  │  • Serilog         │           │  • Serilog       │                    │
│  │  • Prometheus      │           │  • Prometheus    │                    │
│  │  • Swagger/OpenAPI │           └────────┬─────────┘                    │
│  └─────────┬──────────┘                    │                               │
│            │                                │                               │
│            │                                │                               │
│  ┌─────────▼────────────────────────────────▼───────┐                     │
│  │         PostgreSQL 16 Database                   │                     │
│  │         ──────────────────────                   │                     │
│  │  • Tasks, Users, Attachments, AuditLogs          │                     │
│  │  • Refresh Tokens                                │                     │
│  │  • JSONB for Tags                                │                     │
│  └──────────────────────────────────────────────────┘                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
                                 │
                       ┌─────────▼────────────┐
                       │   External Services  │
                       │   ────────────────   │
                       │  • Mailgun API       │
                       └──────────────────────┘
```

### 3. Component Diagram - API (C4 Level 3)

```
┌───────────────────────────────────────────────────────────────────┐
│                        TaskTracker.API                             │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │                    Controllers Layer                          │ │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐             │ │
│  │  │   Auth     │  │   Tasks    │  │Attachments │             │ │
│  │  │Controller  │  │Controller  │  │ Controller │             │ │
│  │  └──────┬─────┘  └──────┬─────┘  └──────┬─────┘             │ │
│  │         │                │                │                   │ │
│  └─────────┼────────────────┼────────────────┼───────────────────┘ │
│            │                │                │                     │
│  ┌─────────▼────────────────▼────────────────▼───────────────────┐ │
│  │              Application Layer (Services)                      │ │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐              │ │
│  │  │   Auth     │  │   Task     │  │Attachment  │              │ │
│  │  │  Service   │  │  Service   │  │  Service   │              │ │
│  │  └──────┬─────┘  └──────┬─────┘  └──────┬─────┘              │ │
│  │         │                │                │                    │ │
│  └─────────┼────────────────┼────────────────┼────────────────────┘ │
│            │                │                │                      │
│  ┌─────────▼────────────────▼────────────────▼────────────────────┐ │
│  │           Infrastructure Layer (Repositories)                   │ │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐               │ │
│  │  │   User     │  │   Task     │  │ Attachment │               │ │
│  │  │Repository  │  │Repository  │  │ Repository │               │ │
│  │  └──────┬─────┘  └──────┬─────┘  └──────┬─────┘               │ │
│  │         │                │                │                     │ │
│  └─────────┼────────────────┼────────────────┼─────────────────────┘ │
│            │                │                │                       │
│  ┌─────────▼────────────────▼────────────────▼─────────────────────┐ │
│  │                   EF Core DbContext                              │ │
│  └──────────────────────────────────────────────────────────────────┘ │
│                              │                                        │
└──────────────────────────────┼────────────────────────────────────────┘
                               ▼
                         PostgreSQL DB

Middleware Pipeline:
────────────────
1. Exception Handler
2. Serilog Request Logging
3. HTTPS Redirection
4. CORS
5. Rate Limiting
6. Authentication (JWT)
7. Authorization
8. Prometheus Metrics
```

### 4. Database Schema Diagram

```
┌─────────────────────────┐
│        Users            │
│─────────────────────────│
│ Id (Guid) PK           │
│ Email (string) UNIQUE   │
│ PasswordHash (string)   │
│ FirstName (string)      │
│ LastName (string)       │
│ CreatedAt (DateTime)    │
│ UpdatedAt (DateTime)    │
│ IsDeleted (bool)        │
└───────────┬─────────────┘
            │
            │ 1:N
            │
┌───────────▼─────────────┐         ┌─────────────────────────┐
│        Tasks            │         │     Attachments         │
│─────────────────────────│         │─────────────────────────│
│ Id (Guid) PK           │◄────────┤ Id (Guid) PK           │
│ UserId (Guid) FK       │  1:N    │ TaskId (Guid) FK       │
│ Title (string)          │         │ FileName (string)       │
│ Description (string)    │         │ FilePath (string)       │
│ Status (enum)           │         │ FileSize (long)         │
│ Priority (enum)         │         │ ContentType (string)    │
│ DueDate (DateTime?)     │         │ UploadedAt (DateTime)   │
│ Tags (JSONB array)      │         │ UploadedByUserId (Guid) │
│ CreatedAt (DateTime)    │         │ IsDeleted (bool)        │
│ UpdatedAt (DateTime)    │         └─────────────────────────┘
│ IsDeleted (bool)        │
└───────────┬─────────────┘
            │
            │ 1:N
            │
┌───────────▼─────────────┐         ┌─────────────────────────┐
│      AuditLogs          │         │    RefreshTokens        │
│─────────────────────────│         │─────────────────────────│
│ Id (Guid) PK           │         │ Id (Guid) PK           │
│ EntityType (string)     │         │ UserId (Guid) FK       │
│ EntityId (string)       │         │ Token (string) UNIQUE   │
│ Action (string)         │         │ ExpiresAt (DateTime)    │
│ Details (string?)       │         │ CreatedAt (DateTime)    │
│ Timestamp (DateTime)    │         │ RevokedAt (DateTime?)   │
│ UserId (Guid?) FK      │         │ IsRevoked (bool)        │
│ TaskId (Guid?) FK      │         └─────────────────────────┘
└─────────────────────────┘

Indexes:
────────
Users: IX_Users_Email
Tasks: IX_Tasks_UserId, IX_Tasks_Status, IX_Tasks_DueDate
Attachments: IX_Attachments_TaskId
AuditLogs: IX_AuditLogs_EntityType, IX_AuditLogs_UserId, IX_AuditLogs_TaskId
RefreshTokens: IX_RefreshTokens_UserId, IX_RefreshTokens_Token
```

### 5. Deployment Architecture (Docker Desktop)

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

### 7. Security Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Security Layers                              │
│                                                                  │
│  1. Transport Security                                          │
│     └─► HTTPS (TLS 1.2+)                                        │
│                                                                  │
│  2. Authentication                                              │
│     ├─► JWT (HS256) - Access Token (60 min)                    │
│     └─► Refresh Token (7 days, stored in DB)                   │
│                                                                  │
│  3. Authorization                                               │
│     ├─► [Authorize] Attribute (ASP.NET Core)                   │
│     ├─► Owner-based Authorization (user can only access own)   │
│     └─► Public Read / Private Write (audit logs)               │
│                                                                  │
│  4. Rate Limiting (3-tier policy)                              │
│     ├─► Per-User: 100 req/min (authenticated)                  │
│     ├─► Per-IP Auth: 20 req/15min (login/register)            │
│     └─► Per-IP Strict: 10 req/min (file uploads)              │
│                                                                  │
│  5. Input Validation                                           │
│     ├─► FluentValidation (server-side)                         │
│     ├─► Model State Validation                                 │
│     └─► File Upload Restrictions (10MB max)                    │
│                                                                  │
│  6. Password Security                                          │
│     ├─► BCrypt Hashing (cost factor 11)                        │
│     └─► Minimum 8 chars, complexity requirements               │
│                                                                  │
│  7. CORS                                                       │
│     └─► Allow specific origins (localhost:3000)                │
│                                                                  │
│  8. Audit Trail                                                │
│     └─► All operations logged (AuditLogs table)                │
└─────────────────────────────────────────────────────────────────┘
```

### 8. Monitoring & Observability

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

## Technology Stack Summary

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Frontend** | React | 18 | UI Framework |
| | TypeScript | 5.x | Type Safety |
| | Vite | 5.x | Build Tool |
| | Tailwind CSS | 3.x | Styling |
| | Axios | 1.x | HTTP Client |
| **Backend API** | .NET | 9.0 | Web Framework |
| | ASP.NET Core | 9.0 | Web API |
| | Entity Framework Core | 9.0 | ORM |
| | JWT Bearer | 9.0 | Authentication |
| | BCrypt.Net | 4.0 | Password Hashing |
| | FluentValidation | 12.x | Input Validation |
| | Serilog | 9.0 | Logging |
| | prometheus-net | 8.2 | Metrics |
| | Swashbuckle | 6.8 | OpenAPI/Swagger |
| **Worker Service** | .NET Worker | 9.0 | Background Jobs |
| | RestSharp | 112.x | HTTP Client (Mailgun) |
| **Database** | PostgreSQL | 16 | Relational DB |
| | Npgsql | 9.0 | Postgres Driver |
| **Infrastructure** | Docker | Latest | Containerization |
| | Docker Compose | Latest | Orchestration |
| **External Services** | Mailgun | API v3 | Email Delivery |

## Additional Documentation

See also:
- [Architecture Rationale](./ARCHITECTURE_RATIONALE.md) - Detailed explanation of design decisions
- [API Documentation](http://localhost:5128/swagger) - Interactive OpenAPI documentation
- [Database Migrations](../TaskTracker.Infrastructure/Migrations/) - EF Core migration history
- [Phase Documentation](../Phases/) - Implementation phases and progress
