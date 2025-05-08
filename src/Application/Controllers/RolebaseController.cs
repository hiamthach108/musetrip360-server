using Application.DTOs.Role;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[Route("/api/v1/roles")]
public class RoleController : ControllerBase
{
  private readonly ILogger<RoleController> _logger;
  private readonly IRolebaseService _service;

  public RoleController(ILogger<RoleController> logger, IRolebaseService service)
  {
    _logger = logger;
    _service = service;
  }

  [Protected]
  [HttpGet]
  public async Task<IActionResult> Get([FromQuery] RoleQuery query)
  {
    _logger.LogInformation("Get all roles request received");
    return await _service.HandleGetAllAsync(query);
  }

  [Protected]
  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(Guid id)
  {
    _logger.LogInformation("Get role by id request received");
    return await _service.HandleGetByIdAsync(id);
  }

  [Protected]
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] RoleCreateDto dto)
  {
    _logger.LogInformation("Create role request received");
    return await _service.HandleCreateAsync(dto);
  }

  [Protected]
  [HttpPut("{id}")]
  public async Task<IActionResult> Update(Guid id, [FromBody] RoleUpdateDto dto)
  {
    _logger.LogInformation("Update role request received");
    return await _service.HandleUpdateAsync(id, dto);
  }
}