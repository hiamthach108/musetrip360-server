namespace Application.Service;

using Application.DTOs.User;
using Application.Shared.Enum;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Users;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Repository;
using Application.DTOs.UserRole;
using Application.DTOs.Role;
using Infrastructure.Cache;
using Application.Shared.Constant;

public interface IUserService
{
  Task<IActionResult> HandleGetAllAsync(UserQuery query);
  Task<IActionResult> HandleGetByIdAsync(Guid id);
  Task<IActionResult> HandleCreateAsync(UserCreateDto dto);
  Task<IActionResult> HandleUpdateAsync(Guid id, UserUpdateDto dto);
  Task<IActionResult> HandleDeleteAsync(Guid id);
  Task<IActionResult> HandleGetProfileAsync();
  Task<IActionResult> HandleChangePassword(ChangePassword req);
  Task<IActionResult> HandleUpdateProfileAsync(UpdateProfileReq req);

  // UserRoles
  Task<IActionResult> HandleGetUserPrivileges();
  Task<IActionResult> HandleGetUserRoles(Guid userId);
  Task<IActionResult> HandleAddUserRole(UserRoleFormDto body);
  Task<IActionResult> HandleDeleteUserRole(UserRoleFormDto body);
  Task<IActionResult> HandleGetMuseumUser(UserRoleQuery query, string museumId);


  Task<Dictionary<string, bool>> GetUserPrivileges(Guid userId);
  Task<bool> ValidatePermission(string museumId, List<string> permissions);
}

public class UserService : BaseService, IUserService
{
  private readonly IUserRepository _userRepo;
  private readonly IUserRoleRepository _userRoleRepo;
  private readonly IRoleRepository _roleRepo;
  private readonly IMuseumRepository _museumRepo;
  private readonly ICacheService _cacheSvc;

  public UserService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpCtx, ICacheService cacheSvc) : base(dbContext, mapper, httpCtx)
  {
    _userRepo = new UserRepository(dbContext);
    _userRoleRepo = new UserRoleRepository(dbContext);
    _roleRepo = new RoleRepository(dbContext);
    _museumRepo = new MuseumRepository(dbContext);
    _cacheSvc = cacheSvc;
  }

  public async Task<IActionResult> HandleCreateAsync(UserCreateDto dto)
  {
    var user = _mapper.Map<User>(dto);

    user.Status = UserStatusEnum.Active;

    await _userRepo.AddAsync(user);

    return SuccessResp.Created("User created successfully");
  }

  public async Task<IActionResult> HandleDeleteAsync(Guid id)
  {
    var user = await _userRepo.GetByIdAsync(id);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    await _userRepo.DeleteAsync(user);

    return SuccessResp.Ok("User deleted successfully");
  }

  public async Task<IActionResult> HandleGetAllAsync(UserQuery query)
  {
    var resp = await _userRepo.GetUserListAsync(query);

    var users = _mapper.Map<IEnumerable<UserDto>>(resp.Users);

    var result = new
    {
      Data = users,
      Total = resp.Total,
      Page = query.Page,
      PageSize = query.PageSize
    };

    return SuccessResp.Ok(result);
  }

  public async Task<IActionResult> HandleGetByIdAsync(Guid id)
  {
    var user = await _userRepo.GetByIdAsync(id);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    var result = _mapper.Map<UserDto>(user);

    return SuccessResp.Ok(result);
  }

  public async Task<IActionResult> HandleUpdateAsync(Guid id, UserUpdateDto dto)
  {
    var user = await _userRepo.GetByIdAsync(id);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    _mapper.Map(dto, user);

    await _userRepo.UpdateAsync(id, user);

    return SuccessResp.Ok("User updated successfully");
  }

  public async Task<IActionResult> HandleGetProfileAsync()
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var user = await _userRepo.GetByIdAsync(payload.UserId);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    var result = _mapper.Map<UserDto>(user);

    return SuccessResp.Ok(result);
  }

  public async Task<IActionResult> HandleChangePassword(ChangePassword req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var user = await _userRepo.GetByIdAsync(payload.UserId);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    if (user.HashedPassword != null && !BCrypt.Net.BCrypt.Verify(req.OldPassword, user.HashedPassword))
    {
      return ErrorResp.BadRequest("Old password is incorrect");
    }

    user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

    await _userRepo.UpdateAsync(user.Id, user);

    return SuccessResp.Ok("Password changed successfully");
  }

  public async Task<IActionResult> HandleUpdateProfileAsync(UpdateProfileReq req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var user = await _userRepo.GetByIdAsync(payload.UserId);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    _mapper.Map(req, user);

    await _userRepo.UpdateAsync(user.Id, user);

    return SuccessResp.Ok("Profile updated successfully");
  }

  public async Task<IActionResult> HandleGetUserPrivileges()
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var user = await _userRepo.GetByIdAsync(payload.UserId);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    var privileges = await GetUserPrivileges(user.Id);
    return SuccessResp.Ok(privileges);
  }

  public async Task<IActionResult> HandleGetUserRoles(Guid userId)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var user = await _userRepo.GetByIdAsync(userId);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    var roles = _userRoleRepo.GetAllByUserId(user.Id);

    var result = _mapper.Map<IEnumerable<UserRoleDto>>(roles);

    return SuccessResp.Ok(result);
  }

  public async Task<IActionResult> HandleAddUserRole(UserRoleFormDto body)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var user = await _userRepo.GetByIdAsync(body.UserId);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    var role = _roleRepo.GetById(body.RoleId);

    if (role == null)
    {
      return ErrorResp.NotFound("Role not found");
    }

    // check is Guid format with museumId
    if (body.MuseumId != null && Guid.TryParse(body.MuseumId, out _))
    {
      var museum = _museumRepo.GetById(Guid.Parse(body.MuseumId));

      if (museum == null)
      {
        return ErrorResp.NotFound("Museum not found");
      }

      var isAllowed = await ValidatePermission(body.MuseumId.ToString(), [PermissionConst.USERS_MANAGEMENT]);
      if (!isAllowed)
      {
        return ErrorResp.Forbidden("You are not allowed to access this resource");
      }
    }
    else
    {
      var isAllowed = await ValidatePermission(PermissionConst.SYSTEM_MUSEUM, [PermissionConst.USERS_MANAGEMENT]);
      if (!isAllowed)
      {
        return ErrorResp.Forbidden("You are not allowed to access this resource");
      }
    }

    var userRole = new UserRole
    {
      UserId = user.Id,
      RoleId = role.Id,
      MuseumId = body.MuseumId
    };

    // check if user role already exists
    var existingUserRole = _userRoleRepo.GetUserRole(user.Id, role.Id, body.MuseumId);
    if (existingUserRole != null)
    {
      return ErrorResp.BadRequest("User role already exists");
    }

    await _userRoleRepo.AddAsync(userRole);

    return SuccessResp.Ok("User role added successfully");
  }

  public async Task<IActionResult> HandleDeleteUserRole(UserRoleFormDto body)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var user = await _userRepo.GetByIdAsync(body.UserId);

    if (user == null)
    {
      return ErrorResp.NotFound("User not found");
    }

    var role = _roleRepo.GetById(body.RoleId);

    if (role == null)
    {
      return ErrorResp.NotFound("Role not found");
    }

    var userRole = _userRoleRepo.GetUserRole(user.Id, role.Id, body.MuseumId);

    if (userRole == null)
    {
      return ErrorResp.NotFound("User role not found");
    }

    await _userRoleRepo.DeleteAsync(userRole);

    return SuccessResp.Ok("User role deleted successfully");
  }

  public async Task<Dictionary<string, bool>> GetUserPrivileges(Guid userId)
  {
    // get from cache first
    var cacheKey = $"users:{userId}:privileges";
    var privilegeCache = await _cacheSvc.Get<Dictionary<string, bool>>(cacheKey);
    if (privilegeCache != null)
    {
      return privilegeCache;
    }

    var roles = _userRoleRepo.GetAllByUserId(userId);

    var privileges = new Dictionary<string, bool>();

    foreach (var role in roles)
    {
      var scope = role.MuseumId ?? "system";
      foreach (var permission in role.Role.Permissions)
      {
        privileges.Add($"{scope}.{permission.Name}", true);
      }
    }

    await _cacheSvc.Set(cacheKey, privileges, TimeSpan.FromMinutes(15));

    return privileges;
  }

  public async Task<bool> ValidatePermission(string museumId, List<string> permissions)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return false;
    }

    // check if user is super admin
    var superAdminKey = $"users:{payload.UserId}:superadmin";
    var superAdminCache = await _cacheSvc.Get<bool?>(superAdminKey);
    if (superAdminCache != null && superAdminCache.Value == true)
    {
      return superAdminCache.Value;
    }

    var isSuperAdmin = _userRoleRepo.IsSuperAdmin(payload.UserId);
    await _cacheSvc.Set(superAdminKey, isSuperAdmin, TimeSpan.FromMinutes(15));
    if (isSuperAdmin)
    {
      return true;
    }

    var privileges = await GetUserPrivileges(payload.UserId);

    foreach (var permission in permissions)
    {
      if (privileges.ContainsKey($"{museumId}.{permission}"))
      {
        return true;
      }
    }

    return false;
  }

  public async Task<IActionResult> HandleGetMuseumUser(UserRoleQuery query, string museumId)
  {
    if (!Guid.TryParse(museumId, out var museumIdGuid))
    {
      return ErrorResp.BadRequest("Invalid museum id");
    }
    var museum = _museumRepo.GetById(museumIdGuid);
    if (museum == null)
    {
      return ErrorResp.NotFound("Museum not found");
    }
    query.MuseumId = museumId;

    var resp = _userRoleRepo.GetListByMuseumId(query);

    var result = new
    {
      Data = _mapper.Map<IEnumerable<UserRoleDto>>(resp.UserRoles),
      Total = resp.Total,
      Page = query.Page,
      PageSize = query.PageSize
    };

    return SuccessResp.Ok(result);
  }
}