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
- `MAILGUN_API_KEY` - Your Mailgun API key
- `MAILGUN_DOMAIN` - Your Mailgun domain
- `POSTGRES_PASSWORD` - Database password
- `JWT_SECRET` - Secret key for JWT tokens

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

### Access the Application

| Service | URL | Description |
|---------|-----|-------------|
| **UI** | http://localhost:3000 | React frontend |
| **API** | http://localhost:5128/swagger | Swagger API docs |
| **API Health** | http://localhost:5128/health | API health check |
| **Worker Health** | http://localhost:5129/health | Worker health check |
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

- ⚠️ `.env` file contains secrets - **NEVER commit to Git**
- ⚠️ Change default passwords in production
- ⚠️ Use strong JWT secret in production
- ⚠️ Configure proper Mailgun API keys
- ⚠️ Update `AllowedHosts` in production

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

1. Update `.env` with production values
2. Change `API_ENVIRONMENT=Production`
3. Use strong passwords and secrets
4. Configure proper domain names
5. Add SSL/TLS certificates
6. Set up monitoring and alerting
7. Configure backup strategy
8. Review security settings

## 📞 Support

If you encounter issues:
1. Check logs: `docker-compose logs`
2. Verify health checks: `docker-compose ps`
3. Review this guide's troubleshooting section
4. Check Docker Desktop dashboard

---

**Happy Containerizing! 🐳**
