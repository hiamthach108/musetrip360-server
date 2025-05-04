# API Coding Convention & Project Structure

## 1. Controller Layer
- **Location:** `src/Application/Controllers`
- **Naming:** Each controller is named after the resource, suffixed with `Controller` (e.g., `UserController`).
- **Namespace:** Follows the pattern `Application.Controllers` or `MuseTrip360.Controllers`.
- **Attributes:**
  - `[ApiController]` for automatic model validation and API-specific behaviors.
  - `[Route("api/v1/[resource]")]` for versioned routing.
- **Dependency Injection:** Services and loggers are injected via constructor.
- **Endpoints:** Use HTTP method attributes (`[HttpGet]`, `[HttpPost]`, etc.) and route templates.
- **Authorization:** Use custom attributes like `[Protected]`, `[AdminOnly]` for access control.
- **Logging:** Log each request at the start of the handler.
- With each Domain in Domain/ you will need a controller and service for that.

## 2. Service Layer
- **Location:** `src/Application/Service`
- **Naming:** Service interfaces and classes are named after the resource (e.g., `IUserService`, `UserService`).
- **Responsibilities:** Business logic, orchestration, and interaction with repositories.
- **Return Type:** Typically returns `Task<IActionResult>` for API responses.
- **Base Class:** May inherit from `BaseService` for shared logic (e.g., extracting JWT payload).
- With each Domain in Domain/ you will need a service for that.
- After implementation you will 

## 3. DTOs (Data Transfer Objects)
- **Location:** `src/Application/DTOs/[Resource]`
- **Naming:** Suffix with `Dto`, `Req`, or `Resp` to indicate purpose (e.g., `UserCreateDto`, `LoginReq`, `LoginResp`).
- **Purpose:** Shape request and response data for each API endpoint.
- **Mapping:** Use AutoMapper profiles for mapping between domain models and DTOs.

## 4. Domain Layer
- **Location:** `src/Domain/[Resource]`
- **Purpose:** Contains core business entities and logic.

## 5. Infrastructure Layer
- **Location:** `src/Infrastructure`
- **Purpose:** Contains database context, repositories, caching, and other infrastructure concerns.
- With each entity in Domain/ you will need to create repository for it

## 6. General Conventions
- **Versioning:** All routes are prefixed with `/api/v1/`.
- **Error Handling:** Use standardized response helpers like `ErrorResp` and `SuccessResp`.
- **Validation:** Use model validation attributes and custom validation logic as needed.
- **Logging:** Log important actions and errors for traceability.

---

### Example: Creating a New API
- Define the Domain first and follow these steps
1. **Create DTOs** in `src/Application/DTOs/[Resource]`.
2. **Add Service Methods** in `src/Application/Service/[Resource]Service.cs` and its interface.
3. **Implement Controller** in `src/Application/Controllers/[Resource]Controller.cs`.
4. **(Optional) Add Domain Models** in `src/Domain/[Resource]` if new business logic/entities are needed.
5. **(Optional) Add Repository Methods** in `src/Infrastructure/Repository` if new data access is required. 