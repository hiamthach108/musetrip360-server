namespace Infrastructure.Repository;

using Application.DTOs.Role;
using Database;
using Domain.Rolebase;
using Microsoft.EntityFrameworkCore;

public interface IRoleRepository
{
  Role? GetById(Guid id);
  RoleList GetRoleList(RoleQuery query);
  Task<Role> AddAsync(Role role);
  Task<Role> UpdateAsync(Guid roleId, Role role);
  Task<Role> DeleteAsync(Role role);
  Role? GetRoleByName(string name);
  Task<Role?> UpdateRolePermissionsAsync(Guid roleId, ICollection<Guid> permissionIdsToAdd, ICollection<Guid> permissionIdsToRemove);
}

public class RoleList
{
  public IEnumerable<Role> Roles { get; set; } = new List<Role>();
  public int Total { get; set; }
}


public class RoleRepository : IRoleRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public RoleRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Role> AddAsync(Role role)
  {
    var result = await _dbContext.Roles.AddAsync(role);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<Role> DeleteAsync(Role role)
  {
    var result = _dbContext.Roles.Remove(role);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public IEnumerable<Role> GetAll()
  {
    return _dbContext.Roles
        .ToList();
  }

  public Role? GetById(Guid id)
  {
    return _dbContext.Roles
        .Include(r => r.Permissions)
        .FirstOrDefault(r => r.Id == id);
  }

  public RoleList GetRoleList(RoleQuery query)
  {
    string searchKeyword = query.Search ?? "";
    int page = query.Page < 1 ? 1 : query.Page;
    int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

    var q = _dbContext.Roles
        .Where(r => r.Name.Contains(searchKeyword) || r.Description.Contains(searchKeyword))
        .AsQueryable();

    var total = q.Count();

    var roles = q
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return new RoleList
    {
      Roles = roles,
      Total = total
    };
  }

  public Role? GetRoleByName(string name)
  {
    return _dbContext.Roles
        .Include(r => r.Permissions)
        .FirstOrDefault(r => r.Name == name);
  }

  public async Task<Role> UpdateAsync(Guid roleId, Role role)
  {
    var existingRole = await _dbContext.Roles
        .Include(r => r.Permissions)
        .FirstOrDefaultAsync(r => r.Id == roleId);

    if (existingRole == null) return null;

    // Update basic properties
    existingRole.Name = role.Name;
    existingRole.Description = role.Description;
    existingRole.IsActive = role.IsActive;

    // Update permissions
    existingRole.Permissions.Clear();
    foreach (var permission in role.Permissions)
    {
      existingRole.Permissions.Add(permission);
    }

    await _dbContext.SaveChangesAsync();
    return existingRole;
  }

  public async Task<Role?> UpdateRolePermissionsAsync(Guid roleId, ICollection<Guid> permissionIdsToAdd, ICollection<Guid> permissionIdsToRemove)
  {
    var existingRole = await _dbContext.Roles
        .Include(r => r.Permissions)
        .FirstOrDefaultAsync(r => r.Id == roleId);

    if (existingRole == null) return null;

    // Remove permissions
    if (permissionIdsToRemove != null && permissionIdsToRemove.Count > 0)
    {
      var permissionsToRemove = existingRole.Permissions
          .Where(p => permissionIdsToRemove.Contains(p.Id))
          .ToList();

      foreach (var permission in permissionsToRemove)
      {
        existingRole.Permissions.Remove(permission);
      }
    }

    // Add permissions
    if (permissionIdsToAdd != null && permissionIdsToAdd.Count > 0)
    {
      var permissionsToAdd = await _dbContext.Permissions
          .Where(p => permissionIdsToAdd.Contains(p.Id))
          .ToListAsync();

      foreach (var permission in permissionsToAdd)
      {
        if (!existingRole.Permissions.Any(p => p.Id == permission.Id))
        {
          existingRole.Permissions.Add(permission);
        }
      }
    }

    await _dbContext.SaveChangesAsync();
    return existingRole;
  }
}