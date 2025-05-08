namespace Application.Service;

using Application.DTOs.Role;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Rolebase;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface IRolebaseService
{
  Task<IActionResult> HandleGetAllAsync(RoleQuery query);
  Task<IActionResult> HandleGetByIdAsync(Guid id);
  Task<IActionResult> HandleCreateAsync(RoleCreateDto dto);
  Task<IActionResult> HandleUpdateAsync(Guid id, RoleUpdateDto dto);
}

public class RolebaseService : BaseService, IRolebaseService
{
  private readonly ILogger<RolebaseService> _logger;
  private readonly IRoleRepository _roleRepo;
  private readonly IMapper _mapper;

  public RolebaseService(
    ILogger<RolebaseService> logger,
    IMapper mapper,
    MuseTrip360DbContext dbContext,
    IHttpContextAccessor httpContextAccessor) : base(dbContext, mapper, httpContextAccessor)
  {
    _logger = logger;
    _roleRepo = new RoleRepository(dbContext);
    _mapper = mapper;
  }

  public async Task<IActionResult> HandleGetAllAsync(RoleQuery query)
  {
    _logger.LogInformation("Getting all roles with query: {@Query}", query);
    var result = _roleRepo.GetRoleList(query);
    return SuccessResp.Ok(result);
  }

  public async Task<IActionResult> HandleGetByIdAsync(Guid id)
  {
    _logger.LogInformation("Getting role by id: {Id}", id);
    var role = _roleRepo.GetById(id);
    if (role == null)
    {
      return ErrorResp.NotFound("Role not found");
    }
    return SuccessResp.Ok(_mapper.Map<RoleDto>(role));
  }

  public async Task<IActionResult> HandleCreateAsync(RoleCreateDto dto)
  {
    _logger.LogInformation("Creating new role: {@Dto}", dto);

    // Check if role name already exists
    var existingRole = _roleRepo.GetRoleByName(dto.Name);
    if (existingRole != null)
    {
      return ErrorResp.BadRequest("Role name already exists");
    }

    var role = _mapper.Map<Role>(dto);
    var result = await _roleRepo.AddAsync(role);
    return SuccessResp.Created(_mapper.Map<RoleDto>(result));
  }

  public async Task<IActionResult> HandleUpdateAsync(Guid id, RoleUpdateDto dto)
  {
    _logger.LogInformation("Updating role {Id} with data: {@Dto}", id, dto);

    var existingRole = _roleRepo.GetById(id);
    if (existingRole == null)
    {
      return ErrorResp.NotFound("Role not found");
    }

    // Check if new name conflicts with other roles
    if (dto.Name != existingRole.Name)
    {
      var roleWithSameName = _roleRepo.GetRoleByName(dto.Name);
      if (roleWithSameName != null)
      {
        return ErrorResp.BadRequest("Role name already exists");
      }
    }

    var role = _mapper.Map<Role>(dto);
    var result = await _roleRepo.UpdateAsync(id, role);
    return SuccessResp.Ok(_mapper.Map<RoleDto>(result));
  }
}