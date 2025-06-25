# Plan Management Implementation Plan for MuseTrip360

## Overview
This document outlines the comprehensive implementation plan for Plan management in the MuseTrip360 system. The Plan entity is part of the subscription system that allows museums to subscribe to different service plans with various features and pricing tiers.

## Current State Analysis

### ✅ Already Implemented
- **Domain Entity**: `src/Domain/Subscription/Plan.cs` - Complete with all required properties
- **Database Configuration**: Plan entity is properly configured in `MuseTrip360DbContext.cs`
- **Relationship**: Plan has one-to-many relationship with Subscription
- **Subscription Status Enum**: `SubscriptionStatusEnum` with Active, Expired, Cancelled values

### ❌ Missing Components
- Plan Repository (IPlanRepository, PlanRepository)
- Plan DTOs (PlanDto, PlanCreateDto, PlanUpdateDto, PlanQuery)
- Plan Service (IPlanService, PlanService)
- Plan Controller (PlanController)
- Plan AutoMapper Profile
- Service registration in Program.cs

## Implementation Plan

### Phase 1: Repository Layer Implementation

#### 1.1 Create Plan Repository Interface
- **Location**: `src/Infrastructure/Repository/PlanRepository.cs`
- **Purpose**: Data access layer for Plan entity following repository pattern

#### 1.2 Repository Features
- Standard CRUD operations (GetById, GetAll, Add, Update, Delete)
- Active plans filtering (`GetActivePlansAsync()`)
- Plans by price range (`GetPlansByPriceRangeAsync(decimal minPrice, decimal maxPrice)`)
- Plans by duration (`GetPlansByDurationAsync(int minDays, int maxDays)`)
- Plan name uniqueness check (`IsNameUniqueAsync(string name, Guid? excludeId)`)

### Phase 2: DTOs Implementation

#### 2.1 Create Plan DTOs
- **Location**: `src/Application/DTOs/Plan/`
- **Files to Create**:
  - `PlanDto.cs` - For retrieving plan data
  - `PlanCreateDto.cs` - For creating new plans
  - `PlanUpdateDto.cs` - For updating existing plans
  - `PlanQuery.cs` - For filtering and pagination
  - `PlanSummaryDto.cs` - For listing plans with minimal data

#### 2.2 AutoMapper Profile
- **Location**: `src/Application/DTOs/Plan/PlanDto.cs`
- **Purpose**: Map between Plan entity and DTOs

### Phase 3: Service Layer Implementation

#### 3.1 Create Plan Service Interface & Implementation
- **Location**: `src/Application/Service/`
- **Files**: `ISubscriptionService.cs`, `SubscriptionService.cs`
- **Inheritance**: PlanService inherits from BaseService

#### 3.2 Service Methods
- `HandleGetAllAsync(PlanQuery query)` - Get all plans with filtering and active plan
- `HandleGetByIdAsync(Guid id)` - Get specific plan
- `HandleCreateAsync(PlanCreateDto dto)` - Create new plan
- `HandleUpdateAsync(Guid id, PlanUpdateDto dto)` - Update existing plan
- `HandleDeleteAsync(Guid id)` - Soft delete plan (set IsActive = false)
- `HandleGetAdminAsync()` - Get all plans with admin management

### Phase 4: Controller Implementation

#### 4.1 Create Plan Controller
- **Location**: `src/Application/Controllers/SubscriptionController.cs`
- **Route**: `/api/v1/plans`
- **Attributes**: `[ApiController]`, `[Protected]` for admin operations

#### 4.2 API Endpoints
- `GET /api/v1/plans` - Get all plans (public)
- `GET /api/v1/plans/admin` - Get admin plans (admin only)
- `GET /api/v1/plans/{id}` - Get specific plan (public)
- `POST /api/v1/plans` - Create plan (admin only)
- `PUT /api/v1/plans/{id}` - Update plan (admin only)
- `DELETE /api/v1/plans/{id}` - Delete plan (admin only)

### Phase 5: Service Registration

#### 5.1 Update Program.cs
- Add Plan service registration
- Add Plan repository registration

## Detailed Implementation Steps

### Step 1: Create Plan Repository

```csharp
// src/Infrastructure/Repository/PlanRepository.cs
public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id);
    Task<IEnumerable<Plan>> GetAllAsync();
    Task<IEnumerable<Plan>> GetActiveAsync();
    Task<Plan> AddAsync(Plan plan);
    Task<Plan> UpdateAsync(Plan plan);
    Task<Plan> DeleteAsync(Plan plan);
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    Task<IEnumerable<Plan>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Plan>> GetByDurationAsync(int minDays, int maxDays);
    Task<IEnumerable<Plan>> GetPopularAsync(int limit = 5);
}

public class PlanRepository : IPlanRepository
{
    private readonly MuseTrip360DbContext _dbContext;

    public PlanRepository(MuseTrip360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Implementation methods...
}
```

### Step 2: Create Plan DTOs

```csharp
// src/Application/DTOs/Plan/PlanDto.cs
public class PlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int? MaxEvents { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    public int SubscriptionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// src/Application/DTOs/Plan/PlanCreateDto.cs
public class PlanCreateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int DurationDays { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? MaxEvents { get; set; }
    
    [Range(0, 100)]
    public decimal? DiscountPercent { get; set; }
    
    public bool IsActive { get; set; } = true;
}

// src/Application/DTOs/Plan/PlanUpdateDto.cs
public class PlanUpdateDto
{
    [StringLength(100)]
    public string? Name { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? DurationDays { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? MaxEvents { get; set; }
    
    [Range(0, 100)]
    public decimal? DiscountPercent { get; set; }
    
    public bool? IsActive { get; set; }
}

// src/Application/DTOs/Plan/PlanQuery.cs
public class PlanQuery
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinDurationDays { get; set; }
    public int? MaxDurationDays { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "Name";
    public string? SortOrder { get; set; } = "asc";
}
```

### Step 3: Create Plan Service

```csharp
// src/Application/Service/IPlanService.cs
public interface IPlanService
{
    Task<IActionResult> HandleGetAllAsync(PlanQuery query);
    Task<IActionResult> HandleGetByIdAsync(Guid id);
    Task<IActionResult> HandleCreateAsync(PlanCreateDto dto);
    Task<IActionResult> HandleUpdateAsync(Guid id, PlanUpdateDto dto);
    Task<IActionResult> HandleDeleteAsync(Guid id);
    Task<IActionResult> HandleGetActiveAsync();
    Task<IActionResult> HandleGetPopularAsync();
}

// src/Application/Service/PlanService.cs
public class PlanService : BaseService, IPlanService
{
    private readonly IPlanRepository _planRepository;

    public PlanService(
        MuseTrip360DbContext dbContext, 
        IMapper mapper, 
        IHttpContextAccessor httpCtx) 
        : base(dbContext, mapper, httpCtx)
    {
        _planRepository = new PlanRepository(dbContext);
    }

    // Implementation methods...
}
```

### Step 4: Create Plan Controller

```csharp
// src/Application/Controllers/PlanController.cs
[ApiController]
[Route("/api/v1/plans")]
public class PlanController : ControllerBase
{
    private readonly ILogger<PlanController> _logger;
    private readonly IPlanService _planService;

    public PlanController(ILogger<PlanController> logger, IPlanService planService)
    {
        _logger = logger;
        _planService = planService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPlans([FromQuery] PlanQuery query)
    {
        _logger.LogInformation("Getting all plans with query: {@Query}", query);
        return await _planService.HandleGetAllAsync(query);
    }

    [HttpGet("admin")]
    public async Task<IActionResult> GetAdminPlans()
    {
        _logger.LogInformation("Getting admin plans");
        return await _planService.HandleGetAdminAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlanById(Guid id)
    {
        _logger.LogInformation("Getting plan by ID: {PlanId}", id);
        return await _planService.HandleGetByIdAsync(id);
    }

    [HttpPost]
    [Protected]
    public async Task<IActionResult> CreatePlan([FromBody] PlanCreateDto dto)
    {
        _logger.LogInformation("Creating new plan: {@Plan}", dto);
        return await _planService.HandleCreateAsync(dto);
    }

    [HttpPut("{id}")]
    [Protected]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] PlanUpdateDto dto)
    {
        _logger.LogInformation("Updating plan {PlanId}: {@Plan}", id, dto);
        return await _planService.HandleUpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Protected]
    public async Task<IActionResult> DeletePlan(Guid id)
    {
        _logger.LogInformation("Deleting plan: {PlanId}", id);
        return await _planService.HandleDeleteAsync(id);
    }
}
```

### Step 5: Service Registration

```csharp
// Program.cs additions
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
```

## Business Logic Features

### 1. Plan Validation
- **Name Uniqueness**: Ensure plan names are unique across the system
- **Price Validation**: Minimum price validation and reasonable maximum limits
- **Duration Validation**: Minimum 1 day, reasonable maximum (e.g., 3650 days for 10 years)
- **Discount Validation**: Discount percentage between 0-100%
- **Max Events Validation**: Reasonable limits for event count

### 2. Plan Status Management
- **Active/Inactive**: Only active plans can be subscribed to
- **Soft Delete**: When deleting, set IsActive = false instead of hard delete
- **Subscription Check**: Prevent deletion of plans with active subscriptions

### 3. Plan Analytics
- **Popular Plans**: Plans with most active subscriptions
- **Revenue Tracking**: Calculate total revenue per plan
- **Usage Statistics**: Track plan adoption rates

## Authorization & Security

### 1. Public Endpoints
- Get all plans (with filtering)
- Get active plans
- Get plan by ID
- Get popular plans

### 2. Protected Endpoints (Admin Only)
- Create plan
- Update plan
- Delete plan

### 3. Validation Rules
- Input validation using Data Annotations
- Business rule validation in service layer
- SQL injection prevention through EF Core

## Error Handling

### 1. Common Errors
- **404 Not Found**: Plan not found
- **400 Bad Request**: Invalid input data
- **409 Conflict**: Duplicate plan name
- **422 Unprocessable Entity**: Business rule violations

### 2. Error Messages
- Clear, user-friendly error messages
- Detailed validation errors for form inputs
- Proper HTTP status codes

## Testing Strategy

### 1. Unit Tests
- Repository methods testing
- Service business logic testing
- DTO validation testing
- AutoMapper profile testing

### 2. Integration Tests
- API endpoint testing
- Database operations testing
- Authorization testing

### 3. Test Data
- Create test plans with various configurations
- Test edge cases (minimum/maximum values)
- Test business rule violations

## Performance Considerations

### 1. Database Optimization
- Proper indexing on Plan.Name (already configured as unique)
- Efficient queries for filtering and sorting
- Pagination for large result sets

### 2. Caching Strategy
- Cache popular plans
- Cache active plans list
- Cache plan statistics

### 3. Query Optimization
- Use appropriate Include() for related data
- Implement projection for summary DTOs
- Optimize count queries for pagination

## Timeline Estimate

### Phase 1: Repository Layer (1 day)
- Create IPlanRepository interface
- Implement PlanRepository with all methods
- Unit tests for repository

### Phase 2: DTOs & AutoMapper (0.5 day)
- Create all Plan DTOs
- Create AutoMapper profile
- Validation attributes

### Phase 3: Service Layer (1 day)
- Create IPlanService interface
- Implement PlanService with business logic
- Error handling and validation

### Phase 4: Controller Layer (0.5 day)
- Create PlanController
- All API endpoints
- Proper logging and error handling

### Phase 5: Integration & Testing (1 day)
- Service registration
- Integration testing
- API testing with Swagger

**Total Estimated Time**: 4 days

## Success Criteria

### 1. Functional Requirements
- ✅ All CRUD operations working correctly
- ✅ Proper filtering and pagination
- ✅ Business rules enforced
- ✅ Authorization working correctly
- ✅ Error handling comprehensive

### 2. Technical Requirements
- ✅ Follows project conventions and patterns
- ✅ Proper logging implementation
- ✅ Database operations optimized
- ✅ AutoMapper configurations correct
- ✅ Swagger documentation complete

### 3. Quality Requirements
- ✅ Code coverage > 80%
- ✅ No security vulnerabilities
- ✅ Performance meets requirements
- ✅ Proper error messages and status codes

## Future Enhancements

### 1. Advanced Features
- Plan comparison functionality
- Plan recommendation engine
- Bulk plan operations
- Plan versioning and history

### 2. Integration Features
- Payment integration for plan purchases
- Notification system for plan changes
- Analytics dashboard for plan performance
- Plan usage tracking and reporting

### 3. Business Features
- Promotional pricing
- Plan bundles and packages
- Custom plan creation for enterprise
- Plan migration and upgrade paths

## Dependencies

### 1. Required Components
- BaseService (already exists)
- MuseTrip360DbContext (already configured)
- AutoMapper (already configured)
- Protected attribute (already exists)
- Error response types (already exist)

### 2. Database Dependencies
- Plan entity (already exists)
- Subscription entity (already exists)
- Database migration (not needed, already configured)

### 3. External Dependencies
- No external API dependencies
- Standard .NET packages already included

## Conclusion

This implementation plan provides a comprehensive approach to implementing Plan management in the MuseTrip360 system. The plan follows all established project conventions and patterns, ensuring consistency with the existing codebase.

The implementation will provide:
- Complete CRUD operations for plan management
- Proper business logic and validation
- Secure API endpoints with appropriate authorization
- Comprehensive error handling and logging
- Scalable and maintainable code structure

Once implemented, this will enable the subscription system to function properly, allowing museums to subscribe to different service plans based on their needs and budget. 