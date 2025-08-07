# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MuseTrip360 is a modern museum management system built with .NET Core 8 using Clean Architecture. It's a comprehensive platform for managing museums, artifacts, events, virtual tours, and user interactions with advanced features like vector search for content discovery and real-time communication.

## Core Technologies

- **.NET Core 8.0**: Main framework
- **PostgreSQL 15**: Database with `pgvector` extension for vector search
- **Entity Framework Core**: ORM with snake_case naming conventions
- **RabbitMQ**: Message queue for asynchronous processing
- **Redis**: Caching and session management
- **Elasticsearch**: Full-text search and vector search capabilities
- **SignalR**: Real-time communication (WebRTC, chat)
- **Docker**: Containerized deployment with docker-compose

## Common Commands

### Development
```bash
# Start development server with hot reload
make start
dotnet watch --environment Development

# Build the application
make build
dotnet build

# Run tests  
make test
dotnet test
```

### Database Operations
```bash
# Create new migration
make new-migration name=MigrationName
dotnet ef migrations add MigrationName --verbose

# Apply migrations
make migration
dotnet ef database update

# Apply migrations in Docker container
make db-migrate
```

### Docker Development
```bash
# Deploy full stack (development)
make deploy

# View logs
make logs-api    # API logs only
make logs        # All service logs

# Container management
make status      # Check container status
make restart-stack  # Restart all containers
make down        # Stop containers (keep data)
make down-volumes   # Stop containers and remove all data
```

## Architecture Overview

### Layer Structure
- **src/Application/**: Controllers, Services, DTOs, Middlewares, Workers
- **src/Domain/**: Entity models organized by domain
- **src/Infrastructure/**: Repository pattern, Database context, Caching
- **src/Core/**: Shared libraries (JWT, Queue, Realtime, etc.)

### Domain Organization
- **Users/**: User management, roles, authentication
- **Museums/**: Museum entities, policies, requests, articles  
- **Events/**: Event management, participants, rooms
- **Artifacts/**: Artifact catalog with categories and historical periods
- **Tours/**: Virtual tours, tour content, tour guides
- **Payment/**: Orders, payments, transactions, wallets
- **Messaging/**: Conversations, messages, notifications
- **Content/**: Categories, historical periods, representation materials

## Key Architectural Patterns

### Entity Structure
All entities inherit from `BaseEntity`:
```csharp
public class BaseEntity
{
    public Guid Id { get; set; }
    public JsonDocument? Metadata { get; set; }  // JSONB for flexibility
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Service Layer Pattern
- All services inherit from `BaseService`
- Use `ExtractPayload()` to get JWT user information
- Return `Task<IActionResult>` for API responses
- Use `SuccessResp` and `ErrorResp` helper classes

### Repository Pattern
- Interface-based repositories in `Infrastructure/Repository/`
- Async operations throughout
- Standard CRUD operations: GetById, GetAll, Add, Update, Delete

### Authentication & Authorization
- JWT-based authentication with Bearer tokens
- `[Protected]` attribute for authenticated endpoints
- Role-based access control through UserRole relationships

## Database Conventions

- **Naming**: snake_case for database tables and columns
- **JSONB**: Used for flexible metadata fields
- **Timestamps**: Auto-managed CreatedAt/UpdatedAt via SaveChangesAsync override
- **Vector Search**: pgvector extension for 768D vectors in embedded_vectors table

## Background Processing

### Workers
Background services in `src/Application/Workers/`:
- `NotificationWorker`: Processes notification queue
- `OrderWorker`: Handles order processing

### Message Queue
- RabbitMQ integration for asynchronous processing
- Vector embedding generation
- Email notifications
- Order processing workflows

## Real-time Features

### SignalR Hubs
- `ChatHub`: Real-time messaging at `/chat`
- `SignalingHub`: WebRTC signaling at `/signaling`

## Development Workflow

When creating new functionality:

1. **Create Entity** in `src/Domain/[Resource]/`
2. **Create Repository** in `src/Infrastructure/Repository/`
3. **Create DTOs** in `src/Application/DTOs/[Resource]/`
4. **Create Service** in `src/Application/Service/`
5. **Create Controller** in `src/Application/Controllers/`
6. **Register services** in `Program.cs`
7. **Create migration** if database changes needed

## Important Conventions

### Code Style
- Follow the patterns in `memory-bank/api_conventions.md`
- No comments in source code by default
- Keep controllers thin, services contain business logic
- Use AutoMapper for entity-DTO mapping

### API Design
- Routes: `/api/v1/[resource]`
- REST conventions: GET, POST, PUT, DELETE
- Standardized responses using SuccessResp/ErrorResp

### Testing
- Swagger UI available at `/swagger`
- Use Bearer token in Authorization header
- Test database operations through Docker containers

## Environment Configuration

Development uses docker-compose with predefined settings. For production, copy `env.template` to `.env` and configure:
- Google OAuth credentials  
- SMTP settings for email
- Payment provider keys
- Database connection strings

## Service Access Points

| Service | URL | Purpose |
|---------|-----|---------|
| API Server | http://localhost:5000 | Main application API |
| Swagger | http://localhost:5000/swagger | API documentation |
| RabbitMQ Management | http://localhost:15672 | Queue monitoring |
| PostgreSQL | localhost:5432 | Database access |
| Redis | localhost:6379 | Cache access |

## Vector Search Implementation

The system supports vector-based similarity search for:
- Artifact recommendations based on descriptions
- Content discovery across museums
- Personalized suggestions using 768-dimensional embeddings

Vector processing is handled asynchronously via RabbitMQ workers that generate embeddings using external Python services.