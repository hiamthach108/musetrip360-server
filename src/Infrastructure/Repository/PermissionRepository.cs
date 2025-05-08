namespace Infrastructure.Repository;

using Application.DTOs.Role;
using Database;
using Domain.Rolebase;
using Microsoft.EntityFrameworkCore;

public interface IPermissionRepository
{
  Permission? GetById(Guid id);
  PermissionList GetPermissionList(PermissionQuery query);
  Task<Permission> AddAsync(Permission permission);
  Task<Permission> UpdateAsync(Guid permissionId, Permission permission);
  Task<Permission> DeleteAsync(Permission permission);
  Permission? GetPermissionByName(string name);
}

public class PermissionList
{
  public IEnumerable<Permission> Permissions { get; set; } = new List<Permission>();
  public int Total { get; set; }
}

public class PermissionRepository : IPermissionRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public PermissionRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Permission> AddAsync(Permission permission)
  {
    var result = await _dbContext.Permissions.AddAsync(permission);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<Permission> DeleteAsync(Permission permission)
  {
    var result = _dbContext.Permissions.Remove(permission);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public IEnumerable<Permission> GetAll()
  {
    return _dbContext.Permissions
        .Include(p => p.Roles)
        .ToList();
  }

  public Permission? GetById(Guid id)
  {
    return _dbContext.Permissions
        .Include(p => p.Roles)
        .FirstOrDefault(p => p.Id == id);
  }

  public PermissionList GetPermissionList(PermissionQuery query)
  {
    string searchKeyword = query.SearchKeyword ?? "";
    int page = query.Page < 0 ? 0 : query.Page;
    int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

    var q = _dbContext.Permissions
        .Include(p => p.Roles)
        .Where(p => p.Name.Contains(searchKeyword) ||
               p.Description.Contains(searchKeyword) ||
               (p.ResourceGroup != null && p.ResourceGroup.Contains(searchKeyword)))
        .AsQueryable();

    var total = q.Count();

    var permissions = q
        .Skip(page * pageSize)
        .Take(pageSize)
        .ToList();

    return new PermissionList
    {
      Permissions = permissions,
      Total = total
    };
  }

  public Permission? GetPermissionByName(string name)
  {
    return _dbContext.Permissions
        .Include(p => p.Roles)
        .FirstOrDefault(p => p.Name == name);
  }

  public async Task<Permission> UpdateAsync(Guid permissionId, Permission permission)
  {
    var existingPermission = await _dbContext.Permissions
        .Include(p => p.Roles)
        .FirstOrDefaultAsync(p => p.Id == permissionId);

    if (existingPermission == null) return null;

    // Update basic properties
    existingPermission.Name = permission.Name;
    existingPermission.Description = permission.Description;
    existingPermission.ResourceGroup = permission.ResourceGroup;
    existingPermission.IsActive = permission.IsActive;

    await _dbContext.SaveChangesAsync();
    return existingPermission;
  }
}