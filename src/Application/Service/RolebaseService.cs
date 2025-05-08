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
  // Role endpoints
  Task<IActionResult> HandleGetAllAsync(RoleQuery query);
  Task<IActionResult> HandleGetByIdAsync(Guid id);
  Task<IActionResult> HandleCreateAsync(RoleCreateDto dto);
  Task<IActionResult> HandleUpdateAsync(Guid id, RoleUpdateDto dto);

  // Permission endpoints
  Task<IActionResult> HandleGetAllPermissionsAsync(PermissionQuery query);
  Task<IActionResult> HandleGetPermissionByIdAsync(Guid id);
  Task<IActionResult> HandleCreatePermissionAsync(PermissionCreateDto dto);
  Task<IActionResult> HandleUpdatePermissionAsync(Guid id, PermissionUpdateDto dto);
  Task<IActionResult> HandleDeletePermissionAsync(Guid id);
}

public class RolebaseService : BaseService, IRolebaseService
{
  private readonly ILogger<RolebaseService> _logger;
  private readonly IRoleRepository _roleRepo;
  private readonly IPermissionRepository _permissionRepo;
  private readonly IMapper _mapper;

  public RolebaseService(
    ILogger<RolebaseService> logger,
    IMapper mapper,
    MuseTrip360DbContext dbContext,
    IHttpContextAccessor httpContextAccessor) : base(dbContext, mapper, httpContextAccessor)
  {
    _logger = logger;
    _roleRepo = new RoleRepository(dbContext);
    _permissionRepo = new PermissionRepository(dbContext);
    _mapper = mapper;
  }

  // Role Management Methods
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

  // Permission Management Methods
  public async Task<IActionResult> HandleGetAllPermissionsAsync(PermissionQuery query)
  {
    _logger.LogInformation("Getting all permissions with query: {@Query}", query);
    var result = _permissionRepo.GetPermissionList(query);
    return SuccessResp.Ok(result);
  }

  public async Task<IActionResult> HandleGetPermissionByIdAsync(Guid id)
  {
    _logger.LogInformation("Getting permission by id: {Id}", id);
    var permission = _permissionRepo.GetById(id);
    if (permission == null)
    {
      return ErrorResp.NotFound("Permission not found");
    }
    return SuccessResp.Ok(_mapper.Map<PermissionDto>(permission));
  }

  public async Task<IActionResult> HandleCreatePermissionAsync(PermissionCreateDto dto)
  {
    _logger.LogInformation("Creating new permission: {@Dto}", dto);

    // Check if permission name already exists
    var existingPermission = _permissionRepo.GetPermissionByName(dto.Name);
    if (existingPermission != null)
    {
      return ErrorResp.BadRequest("Permission name already exists");
    }

    var permission = _mapper.Map<Permission>(dto);
    var result = await _permissionRepo.AddAsync(permission);
    return SuccessResp.Created(_mapper.Map<PermissionDto>(result));
  }

  public async Task<IActionResult> HandleUpdatePermissionAsync(Guid id, PermissionUpdateDto dto)
  {
    _logger.LogInformation("Updating permission {Id} with data: {@Dto}", id, dto);

    var existingPermission = _permissionRepo.GetById(id);
    if (existingPermission == null)
    {
      return ErrorResp.NotFound("Permission not found");
    }

    // Check if new name conflicts with other permissions
    if (dto.Name != existingPermission.Name)
    {
      var permissionWithSameName = _permissionRepo.GetPermissionByName(dto.Name);
      if (permissionWithSameName != null)
      {
        return ErrorResp.BadRequest("Permission name already exists");
      }
    }

    var permission = _mapper.Map<Permission>(dto);
    var result = await _permissionRepo.UpdateAsync(id, permission);
    return SuccessResp.Ok(_mapper.Map<PermissionDto>(result));
  }

  public async Task<IActionResult> HandleDeletePermissionAsync(Guid id)
  {
    _logger.LogInformation("Deleting permission: {Id}", id);

    var permission = _permissionRepo.GetById(id);
    if (permission == null)
    {
      return ErrorResp.NotFound("Permission not found");
    }

    var result = await _permissionRepo.DeleteAsync(permission);
    return SuccessResp.Ok(_mapper.Map<PermissionDto>(result));
  }
}