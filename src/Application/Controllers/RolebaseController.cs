namespace Application.Controllers;

using Application.DTOs.Role;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/rolebase")]
public class RolebaseController : ControllerBase
{
  private readonly ILogger<RolebaseController> _logger;
  private readonly IRolebaseService _service;

  public RolebaseController(ILogger<RolebaseController> logger, IRolebaseService service)
  {
    _logger = logger;
    _service = service;
  }

  [Protected]
  [HttpGet("roles")]
  public async Task<IActionResult> Get([FromQuery] RoleQuery query)
  {
    _logger.LogInformation("Get all roles request received");
    return await _service.HandleGetAllAsync(query);
  }

  [Protected]
  [HttpGet("roles/{id}")]
  public async Task<IActionResult> GetById(Guid id)
  {
    _logger.LogInformation("Get role by id request received");
    return await _service.HandleGetByIdAsync(id);
  }

  [Protected]
  [HttpPost("roles")]
  public async Task<IActionResult> Create([FromBody] RoleCreateDto dto)
  {
    _logger.LogInformation("Create role request received");
    return await _service.HandleCreateAsync(dto);
  }

  [Protected]
  [HttpPut("roles/{id}")]
  public async Task<IActionResult> Update(Guid id, [FromBody] RoleUpdateDto dto)
  {
    _logger.LogInformation("Update role request received");
    return await _service.HandleUpdateAsync(id, dto);
  }

  [Protected]
  [HttpPut("roles/{id}/permissions")]
  public async Task<IActionResult> UpdateRolePermissions(Guid id, [FromBody] RolePermissionUpdateDto dto)
  {
    _logger.LogInformation("Update role permissions request received");
    return await _service.HandleUpdateRolePermissionsAsync(id, dto);
  }

  // Permission endpoints
  [Protected]
  [HttpGet("permissions")]
  public async Task<IActionResult> GetPermissions([FromQuery] PermissionQuery query)
  {
    _logger.LogInformation("Get all permissions request received");
    return await _service.HandleGetAllPermissionsAsync(query);
  }

  [Protected]
  [HttpGet("permissions/{id}")]
  public async Task<IActionResult> GetPermissionById(Guid id)
  {
    _logger.LogInformation("Get permission by id request received");
    return await _service.HandleGetPermissionByIdAsync(id);
  }

  [Protected]
  [HttpPost("permissions")]
  public async Task<IActionResult> CreatePermission([FromBody] PermissionCreateDto dto)
  {
    _logger.LogInformation("Create permission request received");
    return await _service.HandleCreatePermissionAsync(dto);
  }

  [Protected]
  [HttpPut("permissions/{id}")]
  public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] PermissionUpdateDto dto)
  {
    _logger.LogInformation("Update permission request received");
    return await _service.HandleUpdatePermissionAsync(id, dto);
  }

  [Protected]
  [HttpDelete("permissions/{id}")]
  public async Task<IActionResult> DeletePermission(Guid id)
  {
    _logger.LogInformation("Delete permission request received");
    return await _service.HandleDeletePermissionAsync(id);
  }
}