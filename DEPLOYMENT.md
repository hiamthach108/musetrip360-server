# 🚀 MuseTrip360 Deployment Guide

## Quick Start

### 1. Deploy the Full Stack (Development)
```bash
make deploy
```
This will start all services in detached mode. Access your API at: http://localhost:5000

### 2. View Real-time Logs
```bash
make logs-api    # API logs only
make logs        # All service logs
```

### 3. Check Status
```bash
make status      # Container status
make health      # Detailed health status
```

### 4. Stop Services
```bash
make down        # Stop and remove containers (keeps data)
make down-volumes # Stop and remove everything including data
```

## 📋 Complete Command Reference

### Development Commands
- `make start` - Start development server with hot reload
- `make build` - Build the .NET application  
- `make test` - Run tests

### Database Management
- `make new-migration name=MigrationName` - Create new migration
- `make migration` - Apply pending migrations (local)
- `make db-migrate` - Apply migrations in Docker container

### Docker Deployment
- `make deploy` - Deploy full stack (detached mode)
- `make deploy-dev` - Deploy full stack (interactive mode with logs)
- `make start-stack` - Start existing containers
- `make stop-stack` - Stop containers (keep data)
- `make restart-stack` - Restart all containers
- `make down` - Stop and remove containers
- `make down-volumes` - ⚠️ Stop containers and remove ALL data

### Monitoring & Debugging
- `make logs` - Show all container logs
- `make logs-api` - Show API container logs only
- `make logs-db` - Show database logs
- `make logs-redis` - Show Redis logs
- `make logs-elastic` - Show Elasticsearch logs
- `make status` - Show container status
- `make health` - Show detailed health status

### Development Utilities
- `make shell-api` - Enter API container shell
- `make shell-db` - Enter PostgreSQL shell
- `make clean` - Clean unused Docker resources
- `make clean-all` - Deep clean all Docker resources

### Production Deployment
- `make prod-deploy` - Production deployment (requires .env file)
- `make prod-update` - Update production containers
- `make backup-db` - Create database backup
- `make restore-db file=backup.sql` - Restore database from backup

## 🔧 Environment Setup

### For Development
No additional configuration needed! Just run:
```bash
make deploy
```

### For Production
1. **Create environment file:**
   ```bash
   cp env.template .env
   ```

2. **Edit .env with your actual values:**
   ```bash
   # Required for production
   GOOGLE_CLIENT_ID=your-actual-google-client-id
   GOOGLE_CLIENT_SECRET=your-actual-google-secret
   SMTP_EMAIL=your-email@gmail.com
   SMTP_PASSWORD=your-app-password
   # ... other configurations
   ```

3. **Deploy to production:**
   ```bash
   make prod-deploy
   ```

## 🌐 Service Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| **API Server** | http://localhost:5000 | - |
| **Kibana** | http://localhost:5601 | `kibana_admin` / `musetrip360_admin` |
| **Elasticsearch** | http://localhost:9200 | `elastic` / `musetrip360_elastic` |
| **RabbitMQ Management** | http://localhost:15672 | `user` / `password` |
| **PostgreSQL** | localhost:5432 | `postgres` / `123456` |
| **Redis** | localhost:6379 | `exeredis` |

## 🔍 Troubleshooting

### Check if services are running
```bash
make status
```

### View service logs
```bash
make logs-api     # For API issues
make logs-db      # For database issues
make logs-elastic # For search issues
```

### Restart specific service
```bash
docker-compose restart musetrip360-api
docker-compose restart postgres
```

### Clean up and fresh start
```bash
make down-volumes  # ⚠️ This removes all data!
make deploy
```

### Database migration issues
```bash
make shell-api
dotnet ef migrations list
dotnet ef database update
```

## 📦 Common Deployment Scenarios

### Scenario 1: First Time Setup
```bash
# 1. Deploy everything
make deploy

# 2. Check if everything is healthy
make health

# 3. Monitor logs
make logs-api
```

### Scenario 2: Code Updates
```bash
# 1. Stop current deployment
make down

# 2. Rebuild and deploy
make deploy

# 3. Verify deployment
make status
```

### Scenario 3: Database Changes
```bash
# 1. Create migration (local development)
make new-migration name=YourMigrationName

# 2. Deploy with migrations
make deploy
make db-migrate
```

### Scenario 4: Production Updates
```bash
# 1. Backup database
make backup-db

# 2. Update containers
make prod-update

# 3. Verify deployment
make health
```

## 🆘 Emergency Procedures

### Complete Reset (Development)
```bash
make down-volumes
make clean-all
make deploy
```

### Restore from Backup
```bash
make restore-db file=backup_20231201_143022.sql
```

### View All Commands
```bash
make help
```

---

For more help, run `make help` or check the Makefile for command details. 