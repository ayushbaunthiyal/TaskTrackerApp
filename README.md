# Task Tracker Application

A modern full-stack Task Tracker application built with .NET 9 Web API, React TypeScript, PostgreSQL, and Clean Architecture principles.

## Features

### Backend (Phase 1 & 2)
- ✅ Clean Architecture (Domain, Application, Infrastructure, API layers)
- ✅ RESTful API with CRUD operations for tasks
- ✅ JWT Authentication & Authorization
- ✅ User Registration & Login
- ✅ Token Refresh & Revocation
- ✅ PostgreSQL database with Entity Framework Core
- ✅ Advanced Search, filtering, sorting, and pagination
- ✅ Soft delete pattern
- ✅ Audit logging with automatic interceptors
- ✅ Structured logging with Serilog and correlation IDs
- ✅ Health check endpoints
- ✅ Swagger/OpenAPI documentation
- ✅ Global exception handling
- ✅ FluentValidation for input validation
- ✅ Docker support for PostgreSQL

### Frontend (Phase 3)
- ✅ Modern React 18 with TypeScript
- ✅ JWT Authentication UI (Login/Register)
- ✅ Task Management (Create, Edit, Delete, View)
- ✅ Advanced Search & Filtering
- ✅ Real-time Toast Notifications
- ✅ Responsive Design (Mobile, Tablet, Desktop)
- ✅ Due Date Alerts (24-hour highlighting)
- ✅ Status & Priority Color Coding
- ✅ Tag Management
- ✅ Protected Routes
- ✅ Automatic Token Refresh
- ✅ Tailwind CSS Styling
- ✅ Docker Ready

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- IDE (Visual Studio 2022, VS Code, or Rider)

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd TaskTrackerApp
```

### 2. Start PostgreSQL with Docker

```bash
docker-compose up -d
```

This will start a PostgreSQL 16 container with:
- **Host**: localhost
- **Port**: 5433
- **Database**: TaskTrackerDB
- **Username**: tasktracker_user
- **Password**: TaskTracker123!

### 3. Start the Backend API

```bash
cd TaskTracker.API
dotnet run
```

The API will be available at:
- **API**: http://localhost:5128
- **Swagger UI**: http://localhost:5128/swagger

### 4. Start the React UI

```bash
cd task-tracker-ui
npm install
npm run dev
```

The UI will be available at:
- **UI**: http://localhost:3000

### 5. Test the Application

1. Open http://localhost:3000
2. Click "Sign up" to create a new account
3. Login with your credentials
4. Create and manage tasks!

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
└── docker-compose.yml           # PostgreSQL container configuration
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

## Next Steps (Upcoming Features)

- JWT-based authentication
- Role-based authorization
- File upload/download for attachments
- Background worker for reminders
- Rate limiting
- React frontend
- Unit and integration tests

## License

[Specify your license here]
