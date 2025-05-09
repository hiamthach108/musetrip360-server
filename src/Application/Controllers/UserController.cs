namespace MuseTrip360.Controllers;

using Application.DTOs.User;
using Application.DTOs.UserRole;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/users")]
public class UserController : ControllerBase
{

  private readonly ILogger<UserController> _logger;
  private readonly IUserService _service;

  public UserController(ILogger<UserController> logger, IUserService service)
  {
    _logger = logger;
    _service = service;
  }

  [Protected]
  [HttpGet("admin")]
  public async Task<IActionResult> Get([FromQuery] UserQuery query)
  {
    _logger.LogInformation("Get all users request received");

    return await _service.HandleGetAllAsync(query);
  }

  [Protected]
  [HttpPost("admin")]
  public async Task<IActionResult> Create([FromBody] UserCreateDto req)
  {
    _logger.LogInformation("Create user request received");

    return await _service.HandleCreateAsync(req);
  }

  [Protected]
  [HttpPut("admin/{id}")]
  public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateDto req)
  {
    _logger.LogInformation("Update user request received");

    return await _service.HandleUpdateAsync(id, req);
  }

  [Protected]
  [HttpGet("profile")]
  public async Task<IActionResult> GetProfile()
  {
    _logger.LogInformation("Get user profile request received");

    return await _service.HandleGetProfileAsync();
  }

  [Protected]
  [HttpPut("profile")]
  public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileReq req)
  {
    _logger.LogInformation("Update user profile request received");

    return await _service.HandleUpdateProfileAsync(req);
  }

  [Protected]
  [HttpPut("profile/change-password")]
  public async Task<IActionResult> ChangePassword([FromBody] ChangePassword req)
  {
    _logger.LogInformation("Change user password request received");

    return await _service.HandleChangePassword(req);
  }

  [Protected]
  [HttpGet("privileges")]
  public async Task<IActionResult> GetUserRoles()
  {
    _logger.LogInformation("Get user privileges request received");

    return await _service.HandleGetUserPrivileges();
  }

  [Protected]
  [HttpGet("admin/{userId}/roles")]
  public async Task<IActionResult> GetUserRoles(Guid userId)
  {
    _logger.LogInformation("Get user roles request received");

    return await _service.HandleGetUserRoles(userId);
  }

  [Protected]
  [HttpPost("roles")]
  public async Task<IActionResult> AddUserRole([FromBody] UserRoleFormDto req)
  {
    _logger.LogInformation("Add user role request received");

    return await _service.HandleAddUserRole(req);
  }

  [Protected]
  [HttpDelete("roles")]
  public async Task<IActionResult> DeleteUserRole([FromBody] UserRoleFormDto req)
  {
    _logger.LogInformation("Delete user role request received");

    return await _service.HandleDeleteUserRole(req);
  }
}
