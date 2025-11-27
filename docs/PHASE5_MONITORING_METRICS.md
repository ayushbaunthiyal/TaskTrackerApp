# Phase 5: Monitoring, Metrics, Health Checks & Documentation

## Overview

Phase 5 adds comprehensive observability features to the TaskTracker application, including metrics collection, health monitoring, enhanced logging, and complete architecture documentation.

## What Was Implemented

### 1. Metrics Collection (Prometheus)

#### API Metrics
- **HTTP Request Metrics**: Automatic tracking of all HTTP requests (count, duration, status codes)
- **Business Metrics**:
  - Tasks: Created, updated, deleted, completed
  - Active task count (gauge)
  - Authentication: Success/failure rates
  - User registrations
  - Attachments: Uploaded, deleted
  - Task operation duration (histogram)

#### Worker Metrics
- Reminders: Sent, failed, skipped
- Worker cycle duration
- Email quota remaining (daily)
- Last run timestamp
- Worker cycle count

**Endpoint**: `GET http://localhost:5128/metrics`

**Example Prometheus Metrics**:
```prometheus
# HELP tasktracker_tasks_created_total Total number of tasks created
# TYPE tasktracker_tasks_created_total counter
tasktracker_tasks_created_total 150

# HELP tasktracker_tasks_active Current number of active tasks
# TYPE tasktracker_tasks_active gauge
tasktracker_tasks_active 42

# HELP tasktracker_worker_reminders_sent_total Total number of reminders sent
# TYPE tasktracker_worker_reminders_sent_total counter
tasktracker_worker_reminders_sent_total 89
```

### 2. Enhanced Logging

#### Configuration
- **Console Logging**: Real-time log output for development
- **File Logging**: Daily rolling files for persistent storage
  - API logs: `logs/api-{Date}.log`
  - Worker logs: `logs/worker-{Date}.log`

#### Log Levels
- **Information**: Normal operations, startup, shutdown
- **Warning**: Recoverable errors, rate limit hits
- **Error**: Exceptions, failed operations
- **Fatal**: Application crashes

#### Features
- Structured logging (JSON format)
- Correlation IDs for request tracking
- Contextual properties (UserId, RequestPath, Method)

**Example Log Entry**:
```json
{
  "timestamp": "2025-11-27T10:30:45.123Z",
  "level": "Information",
  "messageTemplate": "Task {TaskId} created successfully by user {UserId}",
  "properties": {
    "TaskId": "a1b2c3d4-...",
    "UserId": "e5f6g7h8-...",
    "CorrelationId": "xyz123"
  }
}
```

### 3. Health Checks

#### Endpoints
- **`GET /health`**: Complete health status with all checks
- **`GET /health/db`**: Database connectivity check only

#### Health Checks Included
1. **Database Check**: PostgreSQL connectivity and query execution
2. **Memory Check**: Process memory usage (degraded above 1GB)
3. **Worker Health** (if applicable): Last successful run tracking

#### Response Format
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "duration": "12ms",
      "description": "Database connection established"
    },
    {
      "name": "memory",
      "status": "Healthy",
      "duration": "1ms",
      "description": "Memory usage: 512 MB"
    }
  ]
}
```

### 4. Worker Health Tracking

**WorkerHealthService** monitors background worker status:
- Tracks last successful run timestamp
- Counts failed job executions
- Provides health status (Healthy/Unhealthy)
- Staleness detection (unhealthy if >2 hours since last run)

### 5. Architecture Documentation

#### Created Documents
1. **ARCHITECTURE.md** (500+ lines)
   - 8 detailed diagrams (System Context, Container, Component, Database Schema, Deployment, Data Flows, Security, Monitoring)
   - Technology stack summary
   - Component descriptions
   - Complete system visualization

2. **ARCHITECTURE_RATIONALE.md** (250+ lines)
   - Clean Architecture explanation and benefits
   - Technology choices justification (.NET 9, PostgreSQL, React, JWT, BCrypt)
   - Design patterns used (Repository, DI, Unit of Work, DTO, Service Layer, Options)
   - Security decisions (Defense in Depth, OWASP Top 10 mitigations, password policy, CORS)
   - Scalability considerations (current limitations, horizontal scaling roadmap)
   - Development philosophy (SOLID, DRY, YAGNI, Convention Over Configuration)
   - Decision trade-offs analysis
   - Lessons learned and future improvements

### 6. Integration Tests

Created 3 integration test suites:
- **AuthControllerIntegrationTests**: Register, login, invalid credentials (3 tests)
- **TasksControllerIntegrationTests**: CRUD operations, authorization, filtering, pagination (10 tests)
- **HealthCheckIntegrationTests**: Health endpoint validation (5 tests)

**Total**: 18 integration tests

## How to Use

### View Metrics

```bash
# API metrics
curl http://localhost:5128/metrics

# Metrics include:
# - tasktracker_tasks_created_total
# - tasktracker_tasks_active
# - tasktracker_auth_success_total
# - tasktracker_worker_reminders_sent_total
# - http_requests_received_total
# - http_request_duration_seconds
```

### Check Health Status

```bash
# All health checks
curl http://localhost:5128/health

# Database only
curl http://localhost:5128/health/db
```

### View Logs

```bash
# API logs (PowerShell)
Get-Content logs/api-20251127.log -Tail 50

# Worker logs
Get-Content logs/worker-20251127.log -Tail 50

# Real-time monitoring
Get-Content logs/api-20251127.log -Wait
```

### Run Integration Tests

```bash
# Ensure database is running
docker-compose up -d db

# Run all tests
dotnet test

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run specific test class
dotnet test --filter "FullyQualifiedName~AuthControllerIntegrationTests"
```

## Packages Added

| Package | Version | Purpose |
|---------|---------|---------|
| prometheus-net.AspNetCore | 8.2.1 | Prometheus metrics for ASP.NET Core |
| prometheus-net | 8.2.1 | Core metrics library |
| Serilog.Sinks.File | 6.0.0 | File logging with rolling intervals |
| Serilog.Settings.Configuration | 8.0.4 | Configuration-based Serilog setup |
| Microsoft.Extensions.Diagnostics.HealthChecks | 9.0.0 | Health check infrastructure |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.0 | Integration testing framework |

## Configuration

### Metrics Configuration (appsettings.json)

No special configuration needed - metrics are automatically collected.

### Logging Configuration (appsettings.json)

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/api-.log",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

### Health Check Thresholds

- **Memory**: Degraded above 1GB, Unhealthy above 2GB
- **Worker**: Unhealthy if no successful run in last 2 hours
- **Database**: Timeout after 5 seconds

## Optional: Grafana Dashboard Setup

To visualize metrics, add Grafana + Prometheus:

```bash
# Create docker-compose.monitoring.yml
version: '3.8'
services:
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3001:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana-data:/var/lib/grafana

volumes:
  grafana-data:

# Create prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'tasktracker-api'
    static_configs:
      - targets: ['host.docker.internal:5128']

# Start monitoring stack
docker-compose -f docker-compose.monitoring.yml up -d

# Access:
# - Prometheus: http://localhost:9090
# - Grafana: http://localhost:3001 (admin/admin)
```

## Files Modified/Created

### Created Files
- `TaskTracker.Application/Services/MetricsService.cs`
- `TaskTracker.Worker/Services/WorkerMetricsService.cs`
- `TaskTracker.Worker/Services/WorkerHealthService.cs`
- `docs/architecture/ARCHITECTURE.md`
- `docs/architecture/ARCHITECTURE_RATIONALE.md`
- `TaskTracker.Tests/Integration/AuthControllerIntegrationTests.cs`
- `TaskTracker.Tests/Integration/TasksControllerIntegrationTests.cs`
- `TaskTracker.Tests/Integration/HealthCheckIntegrationTests.cs`

### Modified Files
- `TaskTracker.API/Program.cs` (Prometheus middleware, enhanced health checks)
- `TaskTracker.Worker/Program.cs` (Configuration-based logging, WorkerHealthService registration)
- `TaskTracker.API/Controllers/AuthController.cs` (Metrics tracking)
- `TaskTracker.API/Controllers/TasksController.cs` (Metrics tracking)
- `TaskTracker.API/Controllers/AttachmentsController.cs` (Metrics tracking)
- `TaskTracker.Worker/Services/ReminderService.cs` (Metrics tracking)
- `TaskTracker.Worker/Services/ReminderHostedService.cs` (Health tracking)
- `TaskTracker.API/appsettings.json` (File logging configuration)
- `TaskTracker.API/appsettings.Development.json` (File logging configuration)
- `TaskTracker.API.csproj`, `TaskTracker.Application.csproj`, `TaskTracker.Worker.csproj`, `TaskTracker.Tests.csproj` (Package references)

## Verification

### Build Status
```bash
dotnet build
# Expected: Build succeeded in ~10-20s (0 errors, 0 warnings)
```

### Test Status
```bash
dotnet test
# Expected: 100+ tests passed (88 unit tests + 18 integration tests)
```

### Metrics Endpoint
```bash
curl http://localhost:5128/metrics
# Expected: Prometheus-formatted metrics
```

### Health Endpoint
```bash
curl http://localhost:5128/health
# Expected: {"status":"Healthy","checks":[...]}
```

## Next Steps (Post-Phase 5)

### Phase 6 Candidates
1. **Caching Layer**: Redis for distributed caching
2. **API Versioning**: `/api/v1/` routes
3. **End-to-End Tests**: Playwright for UI testing
4. **Advanced Monitoring**: Grafana dashboards, alerting
5. **Performance Optimization**: Query profiling, database indexes
6. **Feature Enhancements**: Task sharing, real-time updates (SignalR)

## Troubleshooting

### Metrics not showing up
- Ensure `/metrics` endpoint is accessible
- Check Prometheus middleware is registered: `app.UseHttpMetrics()`
- Verify package `prometheus-net.AspNetCore` is installed

### Health check fails
- **Database**: Check PostgreSQL is running (`docker ps`)
- **Memory**: High usage - restart application or increase limits
- **Worker**: Check worker is running and processing reminders

### Logs not writing to file
- Ensure `logs/` directory exists (created automatically)
- Check Serilog configuration in `appsettings.json`
- Verify `Serilog.Sinks.File` package is installed

### Integration tests fail
- Ensure database is running before tests
- Check connection string in test configuration
- Run tests individually to isolate failures

## Summary

Phase 5 successfully implemented:
✅ Prometheus metrics collection (API + Worker)  
✅ Enhanced file logging (daily rolling)  
✅ Health checks with detailed JSON responses  
✅ Worker health tracking  
✅ Comprehensive architecture documentation (ARCHITECTURE.md + ARCHITECTURE_RATIONALE.md)  
✅ Integration tests (18 tests across 3 suites)  
✅ All builds successful (0 errors)  
✅ No breaking changes to existing functionality  

**Production Ready**: All Phase 5 features are ready for local/Docker Desktop deployment.
