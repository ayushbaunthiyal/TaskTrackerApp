# Phase 1: Backend Foundation & Database Setup

**Status**: ✅ Completed  
**Date**: November 26, 2025

## Overview

Phase 1 focused on establishing the backend foundation for the Task Tracker application using Clean Architecture principles, .NET 8 Web API, PostgreSQL database, and comprehensive API functionality.

## Objectives Completed

### 1. Solution Structure ✅
- Created `TaskTracker.sln` with Clean Architecture organization
- **Projects Created**:
  - `TaskTracker.Domain` - Core entities and business logic
  - `TaskTracker.Application` - Business logic, DTOs, interfaces
  - `TaskTracker.Infrastructure` - Data access, EF Core, repositories
  - `TaskTracker.API` - Controllers, middleware, endpoints

### 2. Domain Layer ✅
- **Entities**:
  - `User` - Email, PasswordHash, timestamps, soft delete
  - `TaskItem` - Title, Description, Status, Priority, DueDate, Tags (JSONB)
  - `Attachment` - FileName, FileSize, FilePath, metadata
  - `AuditLog` - Action tracking with timestamps
- **Enums**:
  - `TaskStatus` - Pending, InProgress, Completed, Cancelled
  - `TaskPriority` - Low, Medium, High, Critical
- **Base Classes**:
  - `BaseEntity` - Common properties (Id, CreatedAt, UpdatedAt, IsDeleted)

### 3. Application Layer ✅
- **DTOs**:
  - `TaskDto` - Task data transfer object
  - `CreateTaskDto` - Task creation request
  - `UpdateTaskDto` - Task update request
  - `PaginatedResponse<T>` - Pagination wrapper
  - `TaskFilterDto` - Search/filter parameters
- **Services**:
  - `TaskService` - CRUD operations with pagination
  - `AuditService` - Automatic audit logging
- **Validation**:
  - FluentValidation for DTOs (title length, date validation, tag limits)
- **Interfaces**:
  - Repository interfaces (ITaskRepository, IUserRepository, etc.)
  - Service interfaces (ITaskService, IAuditService)
  - Unit of Work pattern (IUnitOfWork)

### 4. Infrastructure Layer ✅
- **Database**:
  - PostgreSQL 16 via Docker (port 5433)
  - EF Core 8.0.4 with Npgsql provider
  - Dynamic JSON enabled for JSONB support
- **DbContext**:
  - Entity configurations with Fluent API
  - Global query filters for soft delete
  - Indexes on UserId, Status, Priority, DueDate, etc.
- **Repositories**:
  - Generic `Repository<T>` implementation
  - Specific repositories with custom queries
  - `TaskRepository` with advanced filtering
- **Features**:
  - `AuditInterceptor` - Automatic timestamp and audit logging
  - `DbSeeder` - Sample data (3 users, 10 tasks, 3 attachments)
  - Unit of Work with transaction support

### 5. API Project ✅
- **Middleware & Configuration**:
  - Serilog for structured logging with correlation IDs
  - Swagger/OpenAPI documentation
  - Global exception handling (ProblemDetails RFC 7807)
  - CORS policy (allow any origin for development)
  - Health checks (`/health`, `/health/db`)
- **Controllers**:
  - `TasksController` - Full CRUD endpoints
    - GET `/api/tasks` - Paginated list with filters
    - GET `/api/tasks/{id}` - Get by ID
    - POST `/api/tasks` - Create task
    - PUT `/api/tasks/{id}` - Update task
    - DELETE `/api/tasks/{id}` - Soft delete
- **Features**:
  - Auto-apply migrations on startup (development)
  - Auto-seed database on startup (development)
  - Correlation ID enrichment in logs

### 6. Docker Setup ✅
- **PostgreSQL Container**:
  - Image: `postgres:16-alpine`
  - Port: `5433:5432` (mapped to 5433 on host)
  - Database: `TaskTrackerDB`
  - User: `tasktracker_user`
  - Password: `TaskTracker123!`
  - Volume: `postgres_data` for persistence
  - Health checks configured

### 7. Database Schema ✅
- **Migrations**: Initial migration created and applied
- **Tables**:
  - Users (with unique email index)
  - Tasks (with multiple indexes for performance)
  - Attachments
  - AuditLogs
  - __EFMigrationsHistory
- **Relationships**:
  - User → Tasks (one-to-many, cascade delete)
  - Task → Attachments (one-to-many, cascade delete)
  - User → AuditLogs (one-to-many, set null on delete)

### 8. Sample Data ✅
- **Users** (Password: `Password123!`):
  - john.doe@example.com
  - jane.smith@example.com
  - bob.wilson@example.com
- **Tasks**: 10 tasks with varied:
  - Statuses (Pending, InProgress, Completed, Cancelled)
  - Priorities (Low, Medium, High, Critical)
  - Due dates (past, present, future)
  - Tags (various categories)
- **Attachments**: 3 sample file metadata entries

## Technical Decisions

### Architecture Patterns
- **Clean Architecture** - Separation of concerns with dependency inversion
- **Repository Pattern** - Abstraction over data access
- **Unit of Work** - Transaction management
- **CQRS-lite** - Separate DTOs for commands and queries

### Database Choices
- **PostgreSQL** - Enterprise-grade, JSONB support for tags
- **Port 5433** - Avoid conflict with existing PostgreSQL installations
- **JSONB for Tags** - Flexible schema, efficient querying
- **Soft Deletes** - Audit trail and data recovery capability

### API Design
- **RESTful** - Standard HTTP methods and status codes
- **Pagination** - Default 25 items/page, max 100
- **Filtering** - Multiple query parameters for flexibility
- **Sorting** - Configurable field and direction
- **ProblemDetails** - RFC 7807 standard error responses

### Logging & Observability
- **Serilog** - Structured logging with JSON output
- **Correlation IDs** - Request tracing across services
- **Health Checks** - Liveness and database readiness
- **Information Level** - Development environment verbosity

## Configuration

### Connection Strings
```json
"DefaultConnection": "Host=localhost;Port=5433;Database=TaskTrackerDB;Username=tasktracker_user;Password=TaskTracker123!"
```

### Pagination Settings
```json
"Pagination": {
  "DefaultPageSize": 25,
  "MaxPageSize": 100
}
```

## API Endpoints

### Tasks API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tasks` | Get all tasks (paginated, filterable) |
| GET | `/api/tasks/{id}` | Get task by ID |
| POST | `/api/tasks` | Create new task |
| PUT | `/api/tasks/{id}` | Update task |
| DELETE | `/api/tasks/{id}` | Soft delete task |

### Health Checks
| Endpoint | Description |
|----------|-------------|
| `/health` | Overall API health |
| `/health/db` | PostgreSQL connectivity |

### Query Parameters (GET /api/tasks)
- `searchTerm` - Search in title/description
- `status` - Filter by TaskStatus (1-4)
- `priority` - Filter by TaskPriority (1-4)
- `tag` - Filter by specific tag
- `dueDateFrom` - Tasks due after date
- `dueDateTo` - Tasks due before date
- `sortBy` - Sort field (Title, Status, Priority, DueDate, CreatedAt, UpdatedAt)
- `sortDescending` - Sort direction (true/false)
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 25, max: 100)

## NuGet Packages Added

### TaskTracker.Application
- `FluentValidation.DependencyInjectionExtensions` 12.1.0

### TaskTracker.Infrastructure
- `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.4
- `Microsoft.EntityFrameworkCore.Design` 8.0.4
- `BCrypt.Net-Next` 4.0.3

### TaskTracker.API
- `Swashbuckle.AspNetCore` 10.0.1
- `Serilog.AspNetCore` 9.0.0
- `Serilog.Enrichers.CorrelationId` 3.0.1
- `Microsoft.EntityFrameworkCore.Tools` 8.0.4
- `AspNetCore.HealthChecks.NpgSql` 8.0.2

## Running the Application

### Start PostgreSQL
```bash
docker-compose up -d
```

### Run the API
```bash
cd TaskTracker.API
dotnet run
```

### Access Points
- **Swagger UI**: http://localhost:5128/swagger
- **API Base**: http://localhost:5128
- **Health Check**: http://localhost:5128/health
- **DB Health**: http://localhost:5128/health/db

## Issues Resolved

### 1. TaskStatus Naming Conflict
- **Problem**: Ambiguous reference between `TaskTracker.Domain.Enums.TaskStatus` and `System.Threading.Tasks.TaskStatus`
- **Solution**: Used fully qualified namespace `Domain.Enums.TaskStatus`

### 2. JSONB Serialization
- **Problem**: PostgreSQL JSONB required dynamic JSON serialization opt-in
- **Solution**: Added `EnableDynamicJson()` to `NpgsqlDataSourceBuilder`

### 3. Port Conflict
- **Problem**: Port 5432 already in use
- **Solution**: Mapped PostgreSQL to port 5433 on host

### 4. Swagger Package Conflict
- **Problem**: Microsoft.OpenApi version mismatch
- **Solution**: Removed conflicting packages, used default Swagger configuration

## Next Steps (Phase 2+)

### Authentication & Authorization
- [ ] JWT token generation and validation
- [ ] User registration endpoint
- [ ] Login endpoint with token refresh
- [ ] Password change functionality
- [ ] Role-based authorization
- [ ] User can only modify own data (except view all)

### Rate Limiting
- [ ] Install AspNetCoreRateLimit package
- [ ] Configure per-user and per-IP limits
- [ ] Add rate limit headers to responses
- [ ] Implement meaningful error responses

### File Attachments
- [ ] File upload endpoint with validation
- [ ] File download endpoint
- [ ] File deletion with cleanup
- [ ] Storage service abstraction (local/cloud)
- [ ] File size and type restrictions

### Background Worker
- [ ] Create Worker service project
- [ ] Implement reminder check logic (24-hour window)
- [ ] Idempotent reminder processing
- [ ] Hangfire or similar for scheduling
- [ ] Fault tolerance and retry logic

### Testing
- [ ] Unit tests for services and validators
- [ ] Integration tests for API endpoints
- [ ] Repository tests with in-memory DB
- [ ] Health check tests
- [ ] Worker tests

### Frontend
- [ ] React application setup
- [ ] Authentication pages (login, register)
- [ ] Task list with filters and search
- [ ] Task detail/create/edit forms
- [ ] File upload component
- [ ] Change password page

### DevOps
- [ ] Dockerfile for API
- [ ] Docker Compose with all services
- [ ] CI/CD pipeline configuration
- [ ] Environment-specific configurations
- [ ] Production database migrations strategy

## Lessons Learned

1. **Namespace Conflicts**: Be careful with common names like `TaskStatus` that conflict with system namespaces
2. **PostgreSQL JSONB**: Requires explicit opt-in for dynamic JSON serialization in Npgsql 8.x
3. **Package Versions**: Ensure compatibility between related packages (Swagger/OpenAPI)
4. **Clean Architecture**: Strong separation of concerns makes testing and maintenance easier
5. **Audit Interceptor**: Automatic timestamp management reduces boilerplate and ensures consistency

## Success Metrics

- ✅ All 4 projects building successfully
- ✅ Database migrations applied without errors
- ✅ Sample data seeded successfully
- ✅ API running and responding to requests
- ✅ Swagger UI functional and documented
- ✅ Health checks passing
- ✅ Structured logging working with correlation IDs
- ✅ CRUD operations functional via Swagger
- ✅ Soft delete working correctly
- ✅ Pagination and filtering operational

## Files Created

### Configuration Files
- `nuget.config`
- `docker-compose.yml`
- `.dockerignore`
- `appsettings.json`
- `appsettings.Development.json`
- `README.md`

### Domain Layer (7 files)
- `Common/BaseEntity.cs`
- `Enums/TaskStatus.cs`
- `Enums/TaskPriority.cs`
- `Entities/User.cs`
- `Entities/TaskItem.cs`
- `Entities/Attachment.cs`
- `Entities/AuditLog.cs`

### Application Layer (16 files)
- `DTOs/TaskDto.cs`
- `DTOs/CreateTaskDto.cs`
- `DTOs/UpdateTaskDto.cs`
- `DTOs/PaginatedResponse.cs`
- `DTOs/TaskFilterDto.cs`
- `Interfaces/Repositories/IRepository.cs`
- `Interfaces/Repositories/ITaskRepository.cs`
- `Interfaces/Repositories/IUserRepository.cs`
- `Interfaces/Repositories/IAuditLogRepository.cs`
- `Interfaces/Repositories/IAttachmentRepository.cs`
- `Interfaces/Repositories/IUnitOfWork.cs`
- `Interfaces/Services/ITaskService.cs`
- `Interfaces/Services/IAuditService.cs`
- `Services/TaskService.cs`
- `Services/AuditService.cs`
- `Validators/CreateTaskDtoValidator.cs`
- `Validators/UpdateTaskDtoValidator.cs`
- `DependencyInjection.cs`

### Infrastructure Layer (15 files)
- `Data/ApplicationDbContext.cs`
- `Data/DbSeeder.cs`
- `Data/Configurations/UserConfiguration.cs`
- `Data/Configurations/TaskItemConfiguration.cs`
- `Data/Configurations/AttachmentConfiguration.cs`
- `Data/Configurations/AuditLogConfiguration.cs`
- `Data/Interceptors/AuditInterceptor.cs`
- `Repositories/Repository.cs`
- `Repositories/TaskRepository.cs`
- `Repositories/UserRepository.cs`
- `Repositories/AuditLogRepository.cs`
- `Repositories/AttachmentRepository.cs`
- `Repositories/UnitOfWork.cs`
- `DependencyInjection.cs`
- `Migrations/20251125184250_InitialCreate.cs`

### API Layer (3 files)
- `Controllers/TasksController.cs`
- `Middleware/GlobalExceptionHandler.cs`
- `Program.cs`

**Total Files Created**: ~50+ files

## Time Estimate

**Actual Time**: ~2 hours  
**Estimated Effort**: 4-6 hours for experienced developer

## Conclusion

Phase 1 successfully established a solid foundation for the Task Tracker application with:
- Clean, maintainable architecture
- Fully functional CRUD API
- Proper separation of concerns
- Comprehensive logging and error handling
- Docker-based PostgreSQL setup
- Sample data for testing
- API documentation via Swagger

The application is ready for Phase 2 implementation (Authentication, Authorization, and advanced features).
