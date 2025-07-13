# Development Commands
start:
	dotnet watch --environment Development

build:
	dotnet build

test:
	dotnet test

# Migration Commands
new-migration:
	dotnet ef migrations add $(name) --verbose 

migration:
	dotnet ef database update

# Docker Commands - Development
docker-build:
	docker build -t musetrip360_server -f Dockerfile .

docker-run:
	docker run -p 5000:5000 musetrip360_server

# Docker Compose Commands - Full Stack Deployment
deploy:
	@echo "🚀 Deploying MuseTrip360 full stack..."
	@if [ -f .env ]; then \
		echo "✅ Loading environment variables from .env"; \
		docker compose --env-file .env up --build -d; \
	else \
		echo "⚠️  No .env file found, using defaults from env.template"; \
		docker compose --env-file env.template up --build -d; \
	fi
	@echo "✅ Deployment complete!"
	@echo "🌐 API Server: http://localhost:5000"

# Rebuild only the API server container
rebuild-api:
	@echo "🔄 Rebuilding MuseTrip360 API server..."
	@if [ -f .env ]; then \
		docker compose --env-file .env build --no-cache musetrip360-api; \
		docker compose --env-file .env up -d musetrip360-api; \
	else \
		docker compose --env-file env.template build --no-cache musetrip360-api; \
		docker compose --env-file env.template up -d musetrip360-api; \
	fi
	@echo "✅ API server rebuilt and restarted!"

deploy-dev:
	@echo "🔧 Starting development environment..."
	@if [ -f .env ]; then \
		echo "✅ Loading environment variables from .env"; \
		docker compose up --build; \
	else \
		echo "⚠️  No .env file found, using defaults from env.template"; \
		docker compose up --build; \
	fi
	
start-stack:
	@echo "▶️  Starting existing containers..."
	@if [ -f .env ]; then docker compose start; else docker compose start; fi

stop-stack:
	@echo "⏸️  Stopping containers..."
	@if [ -f .env ]; then docker compose stop; else docker compose stop; fi

restart-stack:
	@echo "🔄 Restarting containers..."
	@if [ -f .env ]; then docker compose restart; else docker compose restart; fi

down:
	@echo "🛑 Stopping and removing containers..."
	@if [ -f .env ]; then docker compose down; else docker compose down; fi

down-volumes:
	@echo "🗑️  Stopping containers and removing volumes..."
	@if [ -f .env ]; then docker compose down -v; else docker compose down -v; fi
	@echo "⚠️  All data has been removed!"

# Logs and Monitoring
logs:
	docker compose logs -f

logs-api:
	docker compose logs -f musetrip360-api

logs-db:
	docker compose logs -f postgres

logs-redis:
	docker compose logs -f redis

logs-elastic:
	docker compose logs -f elastic

# Status and Health
status:
	@echo "📊 Container Status:"
	docker compose ps

health:
	@echo "🏥 Health Check Status:"
	docker compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"

# Database Operations (requires running containers)
db-migrate:
	@echo "🔄 Running database migrations in container..."
	docker compose exec musetrip360-api dotnet ef database update

db-seed:
	@echo "🌱 Seeding database..."
	docker compose exec musetrip360-api dotnet run --seed

# Development Helpers
shell-api:
	@echo "🐚 Entering API container shell..."
	docker compose exec musetrip360-api /bin/bash

shell-db:
	@echo "🐚 Entering PostgreSQL container..."
	docker compose exec postgres psql -U postgres -d musetrip360db

# Cleanup Commands
clean:
	@echo "🧹 Cleaning up Docker resources..."
	docker system prune -f

clean-all:
	@echo "🧹 Deep cleaning Docker resources..."
	docker system prune -a -f
	docker volume prune -f

# Production Commands
prod-deploy:
	@echo "🚀 Production deployment..."
	@if [ ! -f .env ]; then \
		echo "❌ Error: .env file not found!"; \
		echo "📝 Please create .env file with production configuration"; \
		exit 1; \
	fi
	docker compose -f docker compose.yml up --build -d
	@echo "✅ Production deployment complete!"

prod-update:
	@echo "📦 Updating production containers..."
	docker compose pull
	docker compose up --build -d --force-recreate
	@echo "✅ Production update complete!"

# Backup and Restore
backup-db:
	@echo "💾 Creating database backup..."
	docker compose exec postgres pg_dump -U postgres musetrip360db > backup_$(shell date +%Y%m%d_%H%M%S).sql
	@echo "✅ Database backup created!"

restore-db:
	@echo "📥 Restoring database from backup..."
	@if [ -z "$(file)" ]; then \
		echo "❌ Error: Please specify backup file with 'make restore-db file=backup_file.sql'"; \
		exit 1; \
	fi
	docker compose exec -T postgres psql -U postgres -d musetrip360db < $(file)
	@echo "✅ Database restored!"

# Help
help:
	@echo "🛠️  MuseTrip360 Makefile Commands"
	@echo ""
	@echo "🔧 Environment:"
	@echo "  setup          - Setup and verify environment variables"
	@echo "  debug          - Debug environment variable loading issues"
	@echo ""
	@echo "📋 Development:"
	@echo "  start          - Start development server with hot reload"
	@echo "  build          - Build the .NET application"
	@echo "  test           - Run tests"
	@echo ""
	@echo "🗄️  Database:"
	@echo "  new-migration  - Create new migration (use: make new-migration name=MigrationName)"
	@echo "  migration      - Apply pending migrations"
	@echo "  db-migrate     - Apply migrations in Docker container"
	@echo ""
	@echo "🐳 Docker - Single Container:"
	@echo "  docker-build   - Build API Docker image"
	@echo "  docker-run     - Run API container standalone"
	@echo ""
	@echo "🚀 Docker Compose - Full Stack:"
	@echo "  deploy         - Deploy full stack (detached mode)"
	@echo "  deploy-dev     - Deploy full stack (interactive mode)"
	@echo "  rebuild-api    - Rebuild only API server container"
	@echo "  start-stack    - Start existing containers"
	@echo "  stop-stack     - Stop containers (keep data)"
	@echo "  restart-stack  - Restart all containers"
	@echo "  down           - Stop and remove containers"
	@echo "  down-volumes   - Stop containers and remove all data"
	@echo ""
	@echo "📊 Monitoring:"
	@echo "  logs           - Show all container logs"
	@echo "  logs-api       - Show API container logs"
	@echo "  logs-db        - Show database logs"
	@echo "  status         - Show container status"
	@echo "  health         - Show detailed health status"
	@echo ""
	@echo "🛠️  Utilities:"
	@echo "  shell-api      - Enter API container shell"
	@echo "  shell-db       - Enter PostgreSQL shell"
	@echo "  clean          - Clean unused Docker resources"
	@echo "  clean-all      - Deep clean all Docker resources"
	@echo ""
	@echo "🏭 Production:"
	@echo "  prod-deploy    - Production deployment with .env validation"
	@echo "  prod-update    - Update production containers"
	@echo "  backup-db      - Create database backup"
	@echo "  restore-db     - Restore database (use: make restore-db file=backup.sql)"
	@echo ""
	@echo "❓ Help:"
	@echo "  help           - Show this help message"

.PHONY: setup debug start build test new-migration migration docker-build docker-run \
        deploy deploy-dev rebuild-api start-stack stop-stack restart-stack down down-volumes \
        logs logs-api logs-db logs-redis logs-elastic status health \
        db-migrate db-seed shell-api shell-db clean clean-all \
        prod-deploy prod-update backup-db restore-db help