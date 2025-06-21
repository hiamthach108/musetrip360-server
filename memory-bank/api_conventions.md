# API Coding Convention & Project Structure

## 1. Project Overview
**MuseTrip360** is a modern museum management system built with .NET Core 8, using Clean Architecture with clearly defined layers.

### Core Technologies:
- **.NET Core 8.0**: Main framework
- **PostgreSQL 15**: Database with `pgvector` extension for vector search
- **Entity Framework Core**: ORM
- **RabbitMQ**: Message queue for asynchronous processing
- **Redis**: Caching
- **Elasticsearch**: Full-text search and vector search
- **SignalR**: Real-time communication
- **AutoMapper**: Object mapping
- **JWT**: Authentication

## 2. Directory Structure

### 2.1 Application Layer (`src/Application/`)
- **Controllers/**: API controllers by domain
- **Service/**: Business logic layer
- **DTOs/**: Data Transfer Objects by domain
- **Workers/**: Background services (BackgroundService)
- **Middlewares/**: Custom middlewares (Protected, AdminOnly, ErrorHandling)
- **Shared/**: Shared components
  - **Constant/**: Constants (Queue, RespCode, Permission, etc.)
  - **Enum/**: Enums (UserStatus, EventType, PaymentStatus, etc.)
  - **Type/**: Base types (BaseEntity, ErrorResp, SuccessResp, etc.)
  - **Helpers/**: Utility helpers (Json, Snowflake, Str)

### 2.2 Domain Layer (`src/Domain/`)
Contains entity models by domain:
- **Users/**: User, UserRole
- **Museums/**: Museum, MuseumPolicy, MuseumRequest, Article
- **Events/**: Event
- **Artifacts/**: Artifact
- **Tickets/**: TicketMaster, TicketAddon, Ticket
- **Tours/**: TourOnline, TourContent, TourGuide
- **Payment/**: Order, OrderTicket, Payment
- **Messaging/**: Message, Conversation, ConversationUser, Notification
- **Feedbacks/**: Feedback, SystemReport
- **Rolebase/**: Role, Permission

### 2.3 Infrastructure Layer (`src/Infrastructure/`)
- **Repository/**: Data access layer for each entity
- **Database/**: DbContext and database configuration
- **Cache/**: Caching services

### 2.4 Core Layer (`src/Core/`)
Shared libraries and services:
- **Queue/**: RabbitMQ integration
- **Realtime/**: SignalR services
- **Jwt/**: JWT authentication services
- **Mail/**: Email services
- **Payos/**: Payment integration
- **Firebase/**: Firebase services
- **Cloudinary/**: Image upload services
- **Crypto/**: Cryptography services
- **Json/**: JSON utilities

## 3. Controller Layer Conventions

### 3.1 Controller Structure
```csharp
[ApiController]
[Route("/api/v1/[resource]")]
public class [Resource]Controller : ControllerBase
{
    private readonly ILogger<[Resource]Controller> _logger;
    private readonly I[Resource]Service _service;

    public [Resource]Controller(ILogger<[Resource]Controller> logger, I[Resource]Service service)
    {
        _logger = logger;
        _service = service;
    }
}
```

### 3.2 Controller Rules
- **Namespace**: `MuseTrip360.Controllers`
- **Route**: `/api/v1/[resource]` (lowercase)
- **Attributes**:
  - `[Protected]`: Requires access token
- **Logging**: Log each request at the start of handler
- **Dependency Injection**: Inject service and logger via constructor

### 3.3 HTTP Methods
- `[HttpGet]`: Retrieve data
- `[HttpPost]`: Create new
- `[HttpPut]`: Update
- `[HttpDelete]`: Delete

## 4. Service Layer Conventions

### 4.1 Service Structure
```csharp
public interface I[Resource]Service
{
    Task<IActionResult> Handle[Action]Async([Parameters]);
}

public class [Resource]Service : BaseService, I[Resource]Service
{
    private readonly I[Resource]Repository _repository;

    public [Resource]Service(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpCtx) 
        : base(dbContext, mapper, httpCtx)
    {
        _repository = new [Resource]Repository(dbContext);
    }
}
```

### 4.2 Service Rules
- **Namespace**: `Application.Service`
- **Inheritance**: Inherit from `BaseService`
- **Method naming**: Start with "Handle" for methods processing from controller
- **Return type**: `Task<IActionResult>` for API responses
- **Response helpers**: Use `SuccessResp` and `ErrorResp`
- **Token extraction**: `var payload = ExtractPayload();` to get user info from JWT

### 4.3 BaseService Features
- **Database context**: `_dbContext`
- **AutoMapper**: `_mapper`
- **HTTP context**: `_httpCtx`
- **JWT payload extraction**: `ExtractPayload()` method

## 5. DTOs Conventions

### 5.1 DTO Structure
- **Location**: `src/Application/DTOs/[Resource]/`
- **Naming**: 
  - `[Resource]Dto`: Basic data
  - `[Resource]CreateDto`: Create new
  - `[Resource]UpdateDto`: Update
  - `[Action]Req`: Request specific
  - `[Action]Resp`: Response specific

### 5.2 AutoMapper Profile
Each DTO folder must have AutoMapper Profile:
```csharp
public class [Resource]Profile : Profile
{
    public [Resource]Profile()
    {
        CreateMap<[Entity], [Resource]Dto>();
        CreateMap<[Resource]CreateDto, [Entity]>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
```

## 6. Repository Layer Conventions

### 6.1 Repository Structure
```csharp
public interface I[Resource]Repository
{
    Task<[Entity]> GetByIdAsync(Guid id);
    Task<IEnumerable<[Entity]>> GetAllAsync();
    Task<[Entity]> AddAsync([Entity] entity);
    Task<[Entity]> UpdateAsync(Guid id, [Entity] entity);
    Task<[Entity]> DeleteAsync([Entity] entity);
}

public class [Resource]Repository : I[Resource]Repository
{
    private readonly MuseTrip360DbContext _dbContext;

    public [Resource]Repository(MuseTrip360DbContext dbContext)
    {
        _dbContext = dbContext;
    }
}
```

### 6.2 Repository Rules
- **Namespace**: `Infrastructure.Repository`
- **Entity Framework**: Use EF Core for database operations
- **Async methods**: All methods are async
- **Standard CRUD**: GetById, GetAll, Add, Update, Delete

## 7. Domain Layer Conventions

### 7.1 Entity Structure
```csharp
public class [Entity] : BaseEntity
{
    // Properties
    public string Name { get; set; } = null!;
    
    // Navigation properties
    public ICollection<[RelatedEntity]> [RelatedEntities] { get; set; } = new List<[RelatedEntity]>();
}
```

### 7.2 BaseEntity
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

## 8. Worker Pattern

### 8.1 Background Services
- **Location**: `src/Application/Workers/`
- **Inheritance**: Inherit from `BackgroundService`
- **Purpose**: Process message queue, scheduled tasks

### 8.2 Worker Structure
```csharp
public class [Name]Worker : BackgroundService
{
    private readonly IQueueSubscriber _queueSubscriber;
    private readonly IServiceScopeFactory _scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Subscribe to queue and process messages
    }
}
```

## 9. Response Conventions

### 9.1 Success Responses
```csharp
SuccessResp.Ok(data);           // 200 with data
SuccessResp.Ok("message");      // 200 with message
SuccessResp.Created(data);      // 201 with data
SuccessResp.NoContent();        // 204
```

### 9.2 Error Responses
```csharp
ErrorResp.BadRequest("message");        // 400
ErrorResp.Unauthorized("message");      // 401
ErrorResp.Forbidden("message");         // 403
ErrorResp.NotFound("message");          // 404
ErrorResp.InternalServerError("message"); // 500
```

## 10. Authentication & Authorization

### 10.1 Middleware Attributes
- `[Protected]`: Requires valid JWT token

### 10.2 JWT Payload
```csharp
var payload = ExtractPayload();
if (payload == null) return ErrorResp.Unauthorized("Invalid token");
var userId = payload.UserId;
```

## 11. Database Conventions

### 11.1 Entity Framework Configuration
- **Connection**: PostgreSQL with pgvector extension
- **Naming**: snake_case for database tables/columns
- **JSONB**: Use for metadata fields
- **Timestamps**: Auto-update CreatedAt/UpdatedAt

### 11.2 Migration
```bash
dotnet ef migrations add [MigrationName]
dotnet ef database update
```

## 12. Implementation Flow

When creating a new API for a domain:

1. **Create Entity** in `src/Domain/[Resource]/`
2. **Create Repository** in `src/Infrastructure/Repository/`
3. **Create DTOs** in `src/Application/DTOs/[Resource]/`
4. **Create Service** in `src/Application/Service/`
5. **Create Controller** in `src/Application/Controllers/`
6. **Register DI** in `Program.cs`
7. **Create Migration** if needed

## 13. Testing Conventions

### 13.1 API Testing
- Use Swagger UI at `/swagger`
- Test with Postman or curl
- Bearer token in Authorization header

### 13.2 Development
```bash
# Run development
dotnet run

# Run with Docker
docker-compose up -d
```

## 14. Error Handling

### 14.1 Global Error Middleware
- `ErrorHandlingMiddleware`: Handle exceptions globally
- Log errors with structured logging
- Return standardized error responses

### 14.2 Validation
- Model validation attributes
- Custom validation in services
- Return BadRequest for validation errors

---

### Important Notes:
- Always follow naming conventions
- Use async/await for database operations
- Implement proper error handling
- Log important actions
- Use dependency injection
- Follow SOLID principles
- Keep controllers thin, services fat 