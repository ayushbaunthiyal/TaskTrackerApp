# 🐳 TaskTracker Docker Deployment Guide

Complete guide to run the entire TaskTracker application stack using Docker.

## 📋 Prerequisites

- Docker Desktop installed and running
- Git (to clone the repository)
- 8GB RAM minimum
- Ports available: 3000, 5128, 5129, 5433

## 🚀 Quick Start

### 1. Clone and Navigate
```powershell
cd d:\repos\mygit\TaskTrackerApp
```

### 2. Configure Environment Variables
The `.env` file is already created with your current settings. Review and update if needed:
```powershell
notepad .env
```

**Important variables to verify:**

**Database Settings:**
- `POSTGRES_DB` - Database name (default: TaskTrackerDB)
- `POSTGRES_USER` - Database user (default: tasktracker_user)
- `POSTGRES_PASSWORD` - Database password (⚠️ change in production!)
- `POSTGRES_HOST_PORT` - Host port for database (default: 5433)

**JWT Authentication:**
- `JWT_SECRET` - Secret key for JWT tokens (⚠️ must be at least 32 characters!)
- `JWT_ISSUER` - Token issuer (default: TaskTrackerAPI)
- `JWT_AUDIENCE` - Token audience (default: TaskTrackerClient)
- `JWT_ACCESS_TOKEN_EXPIRES_MINUTES` - Access token lifetime (default: 60)
- `JWT_REFRESH_TOKEN_EXPIRES_DAYS` - Refresh token lifetime (default: 7)

**Mailgun Email Settings:**
- `MAILGUN_API_KEY` - Your Mailgun API key
- `MAILGUN_DOMAIN` - Your Mailgun domain (e.g., sandbox...mailgun.org)
- `MAILGUN_FROM_EMAIL` - From email address
- `MAILGUN_FROM_NAME` - From name (default: TaskTracker Reminder Service)

**Worker Configuration:**
- `WORKER_CHECK_INTERVAL_MINUTES` - How often to check for tasks (default: 60)
- `WORKER_LOOKAHEAD_HOURS` - How far ahead to look for due tasks (default: 24)
- `WORKER_MAX_EMAILS_PER_RUN` - Max emails per check cycle (default: 50)
- `WORKER_DAILY_EMAIL_QUOTA` - Daily email limit (default: 90)
- `WORKER_ENABLE_EMAIL_SENDING` - Enable/disable emails (default: true)

**Service Ports:**
- `API_PORT` - API port (default: 5128)
- `UI_PORT` - UI port (default: 3000)
- `API_ENVIRONMENT` - Environment (Development/Production)

### 3. Build and Start All Services
```powershell
docker-compose up --build
```

Or run in detached mode (background):
```powershell
docker-compose up --build -d
```

### 4. Wait for Services to Start
The services will start in this order:
1. ✅ PostgreSQL (port 5433)
2. ✅ API (port 5128) - waits for PostgreSQL health check
3. ✅ Worker (port 5129) - waits for API health check
4. ✅ UI (port 3000) - waits for API health check

**Initial startup may take 2-3 minutes** for:
- Building Docker images
- Database migrations
- Data seeding

## 🔍 Verify Everything is Running

### Check Service Status
```powershell
docker-compose ps
```

You should see all 4 services as "Up" and "healthy".

**Expected Output:**
```
NAME                   STATUS              PORTS
tasktracker-api        Up (healthy)        0.0.0.0:5128->5128/tcp
tasktracker-postgres   Up (healthy)        0.0.0.0:5433->5432/tcp
tasktracker-ui         Up                  0.0.0.0:3000->80/tcp
tasktracker-worker     Up (healthy)        0.0.0.0:5129->5129/tcp
```

### Access the Application

| Service | URL | Description |
|---------|-----|-------------|
| **UI** | http://localhost:3000 | React frontend |
| **API** | http://localhost:5128/swagger | Swagger API docs |
| **API Health** | http://localhost:5128/health | API health check |
| **API Metrics** | http://localhost:5128/metrics | Prometheus metrics |
| **Worker Health** | http://localhost:5129/health | Worker health check |
| **Worker Metrics** | http://localhost:5129/metrics | Worker Prometheus metrics |
| **Database** | localhost:5433 | PostgreSQL (use DBeaver/pgAdmin) |

### View Logs
```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f worker
docker-compose logs -f ui
docker-compose logs -f postgres
```

## 🛑 Stop Services

### Stop all services (keeps data)
```powershell
docker-compose down
```

### Stop and remove all data (fresh start)
```powershell
docker-compose down -v
```

### Restart a specific service
```powershell
docker-compose restart api
docker-compose restart worker
```

## 🔧 Common Commands

### Rebuild after code changes
```powershell
# Rebuild specific service
docker-compose up --build api

# Rebuild all
docker-compose up --build
```

### Access container shell
```powershell
# API container
docker exec -it tasktracker-api /bin/bash

# PostgreSQL container
docker exec -it tasktracker-postgres psql -U tasktracker_user -d TaskTrackerDB
```

### View container resource usage
```powershell
docker stats
```

## 📊 Service Details

### PostgreSQL Container
- **Image**: postgres:16-alpine
- **Container**: tasktracker-postgres
- **Port**: 5433 (host) → 5432 (container)
- **Volume**: postgres_data (persistent)
- **Health Check**: Every 10 seconds

### API Container
- **Build**: TaskTracker.API/Dockerfile
- **Container**: tasktracker-api
- **Port**: 5128
- **Depends On**: PostgreSQL (healthy)
- **Health Check**: Every 30 seconds
- **Logs**: api_logs volume

### Worker Container
- **Build**: TaskTracker.Worker/Dockerfile
- **Container**: tasktracker-worker
- **Port**: 5129 (health/metrics)
- **Depends On**: PostgreSQL + API (healthy)
- **Health Check**: Every 60 seconds
- **Logs**: worker_logs volume
- **Function**: Sends email reminders

### UI Container
- **Build**: task-tracker-ui/Dockerfile
- **Container**: tasktracker-ui
- **Port**: 3000 (host) → 80 (container)
- **Depends On**: API (healthy)
- **Reverse Proxy**: Nginx serves React app

## 🐛 Troubleshooting

### Services not starting?
```powershell
# Check logs
docker-compose logs

# Check specific service
docker-compose logs api
```

### Database connection errors?
```powershell
# Verify PostgreSQL is healthy
docker-compose ps postgres

# Check PostgreSQL logs
docker-compose logs postgres

# Ensure port 5433 is not in use
netstat -ano | findstr :5433
```

### API health check failing?
```powershell
# Check API logs
docker-compose logs api

# Test health endpoint manually
curl http://localhost:5128/health

# Restart API
docker-compose restart api
```

### Worker not sending emails?
```powershell
# Check Worker logs
docker-compose logs worker

# Verify Mailgun settings in .env
notepad .env

# Check Worker health
curl http://localhost:5129/health
```

**Common Issues:**
- **Mailgun Sandbox Domain**: Free accounts require authorized recipients. Add your email in Mailgun dashboard.
- **Email Quota Reached**: Worker respects 90 emails/day limit. Check logs for quota messages.
- **No Tasks Due**: Worker only sends reminders for tasks due within 24 hours.
- **Already Sent**: Worker tracks sent reminders via audit log (won't send duplicates).
- **Email Disabled**: Check `WORKER_ENABLE_EMAIL_SENDING=true` in .env

### UI not loading?
```powershell
# Check UI logs
docker-compose logs ui

# Verify API is accessible
curl http://localhost:5128/health

# Clear browser cache and try again
```

### "Port already in use" error?
```powershell
# Check what's using the port
netstat -ano | findstr :5128
netstat -ano | findstr :3000
netstat -ano | findstr :5433

# Kill the process or change port in .env
```

## 🔄 Update Application

### After code changes:
```powershell
# Rebuild and restart affected service
docker-compose up --build -d api

# Or rebuild everything
docker-compose up --build -d
```

### Update environment variables:
```powershell
# Edit .env
notepad .env

# Restart services to pick up changes
docker-compose down
docker-compose up -d
```

## 🗄️ Database Management

### Access PostgreSQL
```powershell
docker exec -it tasktracker-postgres psql -U tasktracker_user -d TaskTrackerDB
```

### Backup Database
```powershell
docker exec tasktracker-postgres pg_dump -U tasktracker_user TaskTrackerDB > backup.sql
```

### Restore Database
```powershell
docker exec -i tasktracker-postgres psql -U tasktracker_user -d TaskTrackerDB < backup.sql
```

### Reset Database (fresh start)
```powershell
docker-compose down -v
docker-compose up -d
```

### Check Database Connection
```powershell
# From inside container
docker exec -it tasktracker-postgres psql -U tasktracker_user -d TaskTrackerDB -c "\dt"

# View all tables
docker exec -it tasktracker-postgres psql -U tasktracker_user -d TaskTrackerDB -c "SELECT tablename FROM pg_tables WHERE schemaname = 'public';"
```

## 📦 Docker Volumes

Persistent data is stored in volumes:
- `postgres_data` - Database data
- `api_logs` - API logs
- `worker_logs` - Worker logs

### List volumes
```powershell
docker volume ls | findstr tasktracker
```

### Remove volumes (deletes all data!)
```powershell
docker-compose down -v
```

## 🔐 Security Notes

- ⚠️ `.env` file contains secrets - **NEVER commit to Git** (already in .gitignore)
- ⚠️ Change default passwords in production (`POSTGRES_PASSWORD`)
- ⚠️ Use strong JWT secret in production (minimum 32 characters)
- ⚠️ Configure proper Mailgun API keys (avoid exposing sandbox keys)
- ⚠️ Update CORS settings in production (currently allows all origins)
- ⚠️ Enable HTTPS in production environments
- ⚠️ Review rate limiting settings for your use case
- ⚠️ Set `WORKER_ENABLE_EMAIL_SENDING=false` in development if testing without emails

## 📈 Monitoring

### Health Checks
- API: http://localhost:5128/health
- Worker: http://localhost:5129/health

### Metrics
- API: http://localhost:5128/metrics (Prometheus format)
- Worker: http://localhost:5129/metrics (Prometheus format)

### Logs
- Real-time: `docker-compose logs -f`
- Stored in Docker volumes: api_logs, worker_logs

## 🎯 Production Deployment

For production deployment:

1. **Environment Configuration**
   - Update `.env` with production values
   - Change `API_ENVIRONMENT=Production`
   - Set strong `POSTGRES_PASSWORD` (16+ characters)
   - Generate secure `JWT_SECRET` (32+ characters)
   - Use production Mailgun domain (not sandbox)

2. **Security Hardening**
   - Enable HTTPS/TLS for all services
   - Configure CORS to allow only your domain
   - Update rate limiting thresholds
   - Use secrets management (Azure Key Vault, AWS Secrets Manager)
   - Enable database encryption at rest

3. **Monitoring & Observability**
   - Set up Prometheus + Grafana for metrics
   - Configure alerts for health check failures
   - Set up log aggregation (ELK, Splunk, or similar)
   - Monitor disk usage for log volumes
   - Track email quota usage

4. **Backup & Recovery**
   - Schedule automated PostgreSQL backups
   - Test restore procedures regularly
   - Store backups in separate location
   - Document recovery procedures

5. **Performance Optimization**
   - Adjust worker intervals based on load
   - Configure connection pooling
   - Enable database query caching
   - Review and optimize slow queries

6. **Deployment Strategy**
   - Use Docker Swarm or Kubernetes for orchestration
   - Implement blue-green or rolling deployments
   - Set up health checks and auto-restart policies
   - Configure resource limits (CPU, memory)

## 🧪 Testing in Docker

### Create Test Tasks
```powershell
# Access API to create tasks due soon
curl -X POST http://localhost:5128/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"email":"john.doe@example.com","password":"Password123!"}'

# Use returned token to create task
curl -X POST http://localhost:5128/api/tasks `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer YOUR_TOKEN" `
  -d '{"title":"Test Task","dueDate":"2025-11-29T10:00:00Z"}'
```

### Verify Worker Processing
```powershell
# Watch worker logs in real-time
docker-compose logs -f worker

# Look for:
# - "Found X tasks needing reminders"
# - "Successfully sent reminder email"
# - "Reminder already sent for task"
```

### Test Email Delivery
1. Add your email as authorized recipient in Mailgun
2. Create task with due date in next 24 hours
3. Wait for worker check interval (default: 60 minutes)
4. Check your email inbox
5. Verify audit log shows reminder sent

## 📊 Resource Usage

**Typical Resource Requirements:**
- **PostgreSQL**: ~100MB RAM, 500MB disk
- **API**: ~150MB RAM, 100MB disk (logs)
- **Worker**: ~100MB RAM, 50MB disk (logs)
- **UI (Nginx)**: ~10MB RAM, 50MB disk
- **Total**: ~400MB RAM, ~700MB disk

**Monitoring Resources:**
```powershell
# Real-time resource usage
docker stats

# Disk usage
docker system df

# Volume sizes
docker volume ls
docker volume inspect tasktracker_postgres_data
```

## 📞 Support

If you encounter issues:
1. Check logs: `docker-compose logs`
2. Verify health checks: `docker-compose ps`
3. Review this guide's troubleshooting section
4. Check Docker Desktop dashboard
5. Verify all required ports are available
6. Ensure .env file is properly configured

## 📚 Additional Resources

- **Phase 5 Documentation**: See `Phases/Phase5_Monitoring_Metrics_HealthChecks_Docker.md`
- **Worker Service Guide**: See `TaskTracker.Worker/README.md`
- **API Documentation**: http://localhost:5128/swagger
- **Project README**: See root `README.md`

---

**Happy Containerizing! 🐳**
