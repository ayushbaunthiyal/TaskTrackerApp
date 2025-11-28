# Task Tracker Application

**A production-ready, enterprise-grade task management system built with modern software engineering practices.**

> From zero to production in 5 phases: Clean Architecture, robust testing, monitoring, and full-stack deployment.

## 🎯 Business Value

**What Problem Does It Solve?**
- Personal and team task organization with intelligent reminders
- Secure file attachments with complete audit trails
- Multi-user collaboration with owner-based access control
- Real-time task tracking with email notifications for upcoming deadlines

**Key Business Features:**
- 📋 **Task Management**: Create, organize, and track tasks with priorities, statuses, tags, and due dates
- 📎 **File Attachments**: Upload and manage documents directly on tasks (10MB limit)
- 🔔 **Smart Reminders**: Automatic email notifications 24 hours before task due dates
- 📊 **Audit Trail**: Complete history of all task changes and user actions
- 🔐 **Secure Access**: JWT-based authentication with automatic token refresh
- 🚦 **Rate Protection**: Built-in rate limiting prevents abuse and ensures fair usage

## 🏗️ Engineering Excellence

### Architecture & Design Patterns
- **Clean Architecture**: Domain-driven design with clear separation of concerns (Domain → Application → Infrastructure → API)
- **SOLID Principles**: Maintainable, testable, and extensible codebase
- **Repository Pattern**: Abstracted data access with dependency injection
- **CQRS-lite**: Separate read/write operations for optimal performance
- **Soft Delete**: Data retention with logical deletion
- **Interceptor Pattern**: Automatic audit logging and timestamp management

### Technology Stack

**Backend (.NET 9)**
- ASP.NET Core Web API with OpenAPI/Swagger documentation
- Entity Framework Core 9 with PostgreSQL 16
- Serilog for structured logging with correlation IDs
- FluentValidation for input validation
- prometheus-net for metrics collection
- Background Worker Service for scheduled tasks

**Frontend (React 18 + TypeScript)**
- Vite 5.0 for lightning-fast builds
- React Router 6 for client-side routing
- Axios with interceptors for API communication
- Tailwind CSS 3.3 for responsive UI
- React Hot Toast for notifications

**Infrastructure**
- Docker Compose for orchestrated deployment
- PostgreSQL with health checks and connection resilience
- Nginx for static file serving and reverse proxy
- Multi-stage Docker builds for optimized images

## 📈 Project Evolution: Phase 1 → Phase 5

### Phase 1: Foundation & Core API (Nov 2025)
**Business Goal:** Establish robust backend infrastructure  
**Engineering Achievements:**
- ✅ Clean Architecture implementation (4 layers)
- ✅ PostgreSQL database with EF Core migrations
- ✅ RESTful API with full CRUD operations
- ✅ Advanced filtering, sorting, and pagination
- ✅ Soft delete pattern for data retention
- ✅ Global exception handling middleware

### Phase 2: Security & Quality (Nov 2025)
**Business Goal:** Enterprise-grade security and data validation  
**Engineering Achievements:**
- ✅ JWT authentication with refresh tokens
- ✅ BCrypt password hashing
- ✅ FluentValidation for request validation
- ✅ Automatic audit logging (interceptor pattern)
- ✅ Structured logging with Serilog
- ✅ Correlation IDs for request tracing
- ✅ Health check endpoints (API + Database)

### Phase 3: Modern UI & User Experience (Nov 27, 2025)
**Business Goal:** Intuitive, responsive user interface  
**Engineering Achievements:**
- ✅ React 18 + TypeScript SPA
- ✅ JWT authentication flow with auto-refresh
- ✅ Protected routes and role-based rendering
- ✅ Advanced search with real-time filtering
- ✅ Responsive design (mobile/tablet/desktop)
- ✅ Toast notifications for user feedback
- ✅ Due date alerts (24-hour visual highlighting)

### Phase 4: Advanced Features & Protection (Nov 27, 2025)
**Business Goal:** Enhanced functionality and security  
**Engineering Achievements:**
- ✅ File upload/download with ownership validation
- ✅ Complete audit trail UI (timeline view)
- ✅ Rate limiting (3-tier: per-user, per-IP auth, per-IP strict)
- ✅ Change password functionality
- ✅ Rate limit testing tool (console app with Spectre.Console)
- ✅ Confirmation dialogs for destructive actions

### Phase 5: Production Readiness (Nov 28, 2025) 🎉
**Business Goal:** Observable, scalable, production-grade deployment  
**Engineering Achievements:**
- ✅ **Monitoring & Metrics**: Prometheus metrics on /metrics endpoints
- ✅ **Health Checks**: Comprehensive health monitoring for all services
- ✅ **Structured Logging**: Serilog with console + file sinks (daily rolling)
- ✅ **Background Worker**: Email reminder service with Mailgun integration
  - Smart 24-hour lookahead scheduling
  - Idempotent delivery (one reminder per task)
  - Quota management (90 emails/day)
- ✅ **Docker Deployment**: Full-stack orchestration with docker-compose
  - PostgreSQL 16 with health checks
  - API service with metrics and health endpoints
  - Worker service with background task processing
  - React UI with Nginx (multi-stage build)
- ✅ **Comprehensive Testing**: 113 total tests
  - Unit tests for services and validators
  - Integration tests for API endpoints
  - Repository tests with in-memory database
- ✅ **Documentation**: Architecture decision records and setup guides

## 📊 Key Metrics

| Metric | Value |
|--------|-------|
| **Total Tests** | 113 (Unit + Integration + Repository) |
| **Code Coverage** | API Controllers, Services, Validators |
| **API Endpoints** | 15+ (Tasks, Auth, Attachments, Audit, Health) |
| **React Components** | 11 (Login, Register, TaskList, TaskForm, etc.) |
| **Architecture Layers** | 4 (Domain, Application, Infrastructure, API) |
| **Database Tables** | 4 (Users, Tasks, Attachments, AuditLogs) |
| **Docker Services** | 4 (PostgreSQL, API, Worker, UI) |
| **Lines of Code** | ~15,000+ across all projects |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- IDE (Visual Studio 2022, VS Code, or Rider)

## 🚀 Quick Start

### Option 1: Full Docker Deployment (Recommended)

```powershell
# Clone and navigate
git clone <repository-url>
cd TaskTrackerApp

# Start all services (PostgreSQL, API, Worker, UI)
docker-compose up --build -d

# Access the application
# UI: http://localhost:3000
# API: http://localhost:5128
# Swagger: http://localhost:5128/swagger
# Metrics: http://localhost:5128/metrics
# Health: http://localhost:5128/health

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

### Option 2: Local Development

**1. Start PostgreSQL**
```powershell
docker-compose up -d postgres
```

**2. Start Backend API**
```powershell
cd TaskTracker.API
dotnet run
# Available at: http://localhost:5128
```

**3. Start Worker Service (Optional)**
```powershell
cd TaskTracker.Worker
# Configure Mailgun settings in appsettings.json first
dotnet run
```

**4. Start React UI**
```powershell
cd task-tracker-ui
npm install
npm run dev
# Available at: http://localhost:3000
```

**5. Test the Application**
1. Open http://localhost:3000
2. Register: john@example.com / Password123!
3. Create tasks, upload files, set due dates
4. Check email for 24-hour reminders (if worker is running)

```bash
docker-compose up -d
```

This will start a PostgreSQL 16 container with:
- **Host**: localhost
- **Port**: 5432
- **Database**: TaskTrackerDB
- **Username**: tasktracker_user
- **Password**: TaskTracker123!

### 3. Run the API

```bash
cd TaskTracker.API
dotnet run
```

The API will:
- Automatically apply database migrations
- Seed sample data (3 users, 10 tasks, 3 attachments)
- Start on `https://localhost:7xxx` and `http://localhost:5xxx`

### 4. Access Swagger UI

Open your browser and navigate to:
```
https://localhost:7xxx/swagger
```

## API Endpoints

### Tasks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tasks` | Get all tasks with filtering, sorting, pagination |
| GET | `/api/tasks/{id}` | Get task by ID |
| POST | `/api/tasks` | Create a new task |
| PUT | `/api/tasks/{id}` | Update an existing task |
| DELETE | `/api/tasks/{id}` | Delete a task (soft delete) |

### Health Checks

| Endpoint | Description |
|----------|-------------|
| `/health` | Overall API health |
| `/health/db` | Database connectivity check |

### Query Parameters for GET /api/tasks

- `searchTerm` - Search in title and description
- `status` - Filter by status (1=Pending, 2=InProgress, 3=Completed, 4=Cancelled)
- `priority` - Filter by priority (1=Low, 2=Medium, 3=High, 4=Critical)
- `tag` - Filter by tag
- `dueDateFrom` - Filter tasks with due date after this date
- `dueDateTo` - Filter tasks with due date before this date
- `sortBy` - Sort field (Title, Status, Priority, DueDate, CreatedAt, UpdatedAt)
- `sortDescending` - Sort direction (true/false)
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 25, max: 100)

## Project Structure

```
TaskTrackerApp/
├── TaskTracker.Domain/          # Entities, Enums, Common classes
│   ├── Common/
│   ├── Entities/
│   └── Enums/
├── TaskTracker.Application/     # Business logic, DTOs, Interfaces
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   └── Validators/
├── TaskTracker.Infrastructure/  # Data access, EF Core, Repositories
│   ├── Data/
│   ├── Repositories/
│   └── DependencyInjection.cs
├── TaskTracker.API/             # Controllers, Middleware, Program.cs
│   ├── Controllers/
│   ├── Middleware/
│   └── Program.cs
├── TaskTracker.Worker/          # Background service for reminders (NEW!)
│   ├── Services/
│   │   ├── MailgunEmailService.cs
│   │   ├── ReminderService.cs
│   │   └── ReminderHostedService.cs
│   ├── Configuration/
│   ├── Program.cs
│   └── README.md
├── task-tracker-ui/             # React TypeScript frontend
│   ├── src/
│   │   ├── components/
│   │   ├── context/
│   │   └── types/
│   └── package.json
└── docker-compose.yml           # Full stack deployment
```

## Database Schema

### Users
- Id (Guid, PK)
- Email (string, unique)
- PasswordHash (string)
- CreatedAt, UpdatedAt, IsDeleted

### Tasks
- Id (Guid, PK)
- UserId (Guid, FK)
- Title (string, max 200)
- Description (string, max 2000)
- Status (int: 1-4)
- Priority (int: 1-4)
- DueDate (DateTime, nullable)
- Tags (jsonb array)
- CreatedAt, UpdatedAt, IsDeleted

### Attachments
- Id (Guid, PK)
- TaskId (Guid, FK)
- FileName, FileSize, FilePath
- UploadedAt, CreatedAt, UpdatedAt, IsDeleted

### AuditLogs
- Id (Guid, PK)
- UserId (Guid, FK, nullable)
- Action, EntityType, EntityId
- Timestamp, Details

## Sample Data

The application seeds the following data in development:

- **3 Users**: john.doe@example.com, jane.smith@example.com, bob.wilson@example.com
  - Password for all: `Password123!`
- **10 Tasks**: Various statuses, priorities, and due dates
- **3 Attachments**: Sample file metadata

## Technologies

- **.NET 8** - Web API framework
- **Entity Framework Core 8** - ORM
- **PostgreSQL 16** - Database
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **FluentValidation** - Input validation
- **BCrypt.Net** - Password hashing

## Development

### Run Migrations

```bash
# From solution root
dotnet ef migrations add MigrationName --project TaskTracker.Infrastructure --startup-project TaskTracker.API

# Apply migrations
dotnet ef database update --project TaskTracker.Infrastructure --startup-project TaskTracker.API
```

### Stop PostgreSQL

```bash
docker-compose down
```

### Remove PostgreSQL Data (Reset)

```bash
docker-compose down -v
```

## Background Worker Service (NEW!)

The TaskTracker.Worker service sends email reminders for upcoming tasks.

### Features

- **Scheduled Checks**: Runs every 30-60 minutes
- **Email Notifications**: Beautiful HTML emails via Mailgun
- **Idempotency**: Each task gets ONE reminder (tracked via audit log)
- **Smart Filtering**: Only tasks due within 24 hours
- **Quota Management**: Respects 90 emails/day limit
- **Docker Ready**: Fully containerized

### Quick Start

```powershell
# 1. Start PostgreSQL
docker-compose up -d postgres

# 2. Configure Mailgun
# Add your API key and domain to TaskTracker.Worker/appsettings.json

# 3. Add authorized recipient
# Go to Mailgun dashboard and authorize your email

# 4. Run worker
cd TaskTracker.Worker
dotnet run
```

### Configuration

See `TaskTracker.Worker/README.md` for detailed configuration options.

Key settings in `appsettings.json`:

```json
{
  "MailgunSettings": {
    "ApiKey": "your-mailgun-api-key",
    "Domain": "your-mailgun-domain"
  },
  "WorkerSettings": {
    "CheckIntervalMinutes": 30,
    "DueDateLookaheadHours": 24,
    "DailyEmailQuota": 90
  }
}
```

### How It Works

1. Worker runs every N minutes (configurable)
2. Finds tasks due within 24 hours that haven't received reminders
3. Sends beautiful HTML email to task owner
4. Logs reminder in audit trail (prevents duplicates)
5. Respects daily email quota (stops at 90/day)

### Testing

```powershell
# Run test script
cd TaskTracker.Worker
.\test-worker.ps1

# Create test tasks with due dates in next 24 hours
# Check your email for reminders
# Verify no duplicates are sent
```

## Running with Docker

### Start Full Stack

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

Services available:
- **PostgreSQL**: localhost:5433
- **API**: localhost:5128
- **Worker**: Background service (no ports)

## 🧪 Testing

### Run All Tests
```powershell
# Run all 113 tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Suites
- **Unit Tests**: Services, validators, utilities (TaskTracker.Application.Tests)
- **Integration Tests**: API endpoints with in-memory database (TaskTracker.API.Tests)
- **Repository Tests**: Data access layer validation (TaskTracker.Infrastructure.Tests)

### Rate Limit Testing
```powershell
cd TaskTracker.RateLimitTester
dotnet run
# Interactive menu to test rate limiting policies
```

## 📊 Monitoring & Observability

### Prometheus Metrics
Visit `http://localhost:5128/metrics` to see:
- HTTP request counts and durations
- Active requests
- Error rates
- Custom business metrics

### Health Checks
- **API Health**: http://localhost:5128/health
- **Database Health**: http://localhost:5128/health/db

### Logs
- **Console**: Real-time colored output with correlation IDs
- **File**: Daily rolling logs in `logs/` directory
- **Format**: Structured JSON for easy parsing

## 🎓 Key Learnings & Best Practices

### What Worked Well
1. **Clean Architecture**: Easy to test, maintain, and extend
2. **Type Safety**: TypeScript + FluentValidation caught bugs early
3. **Docker Compose**: Simplified multi-service orchestration
4. **Audit Interceptors**: Automatic logging without code duplication
5. **Rate Limiting**: Built-in .NET 9 features work excellently
6. **Prometheus Metrics**: Simple integration, powerful insights

### Design Decisions
- **Soft Delete**: Retain data for compliance and auditing
- **JWT in localStorage**: Acceptable for this use case, refresh tokens mitigate risks
- **Mailgun Free Tier**: Sufficient for proof-of-concept, limited to 90 emails/day
- **Fixed Window Rate Limiting**: Simple, predictable, adequate for most scenarios
- **Serilog File Logging**: Daily rolling prevents unbounded disk usage

## 📚 Documentation

- **Phase 1**: Foundation & Core API - See `Phases/Phase1_Summary.md`
- **Phase 2**: Security & Validation - See `Phases/Phase2_Summary.md`
- **Phase 3**: React UI Implementation - See `Phases/Phase3_React_UI_Implementation.md`
- **Phase 4**: Advanced Features - Included in Phase 3 documentation
- **Phase 5**: Production Readiness - See `Phases/Phase5_Monitoring_Metrics_HealthChecks_Docker.md`
- **Worker Service**: Background Jobs - See `TaskTracker.Worker/README.md`
- **Rate Limiting**: Testing Guide - See `Phases/RATE_LIMITING_QUICK_START.md`

## 👥 Contributing

This is a learning project demonstrating modern full-stack development practices. Feel free to:
- Fork and experiment
- Suggest improvements via issues
- Submit pull requests
- Use as reference for your own projects

## 📄 License

[Ayush Baunthiyal]

---

**Built with ❤️ using .NET 9, React 18, PostgreSQL, and Docker**

*Status: ✅ Production Ready | Phase 5 Complete | 113 Tests Passing*
