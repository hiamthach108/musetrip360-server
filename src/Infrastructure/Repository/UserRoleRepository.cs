namespace Infrastructure.Repository;

using Application.Shared.Constant;
using Database;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

public interface IUserRoleRepository
{
  Task<UserRole> AddAsync(UserRole userRole);
  Task<UserRole> DeleteAsync(UserRole userRole);
  UserRole? GetUserRole(Guid userId, Guid roleId, string? museumId);
  IEnumerable<UserRole> GetAllAsync();
  IEnumerable<UserRole> GetAllByUserId(Guid userId);
  bool IsSuperAdmin(Guid userId);
}

public class UserRoleRepository : IUserRoleRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public UserRoleRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<UserRole> AddAsync(UserRole userRole)
  {
    var result = await _dbContext.UserRoles.AddAsync(userRole);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<UserRole> DeleteAsync(UserRole userRole)
  {
    var result = _dbContext.UserRoles.Remove(userRole);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public IEnumerable<UserRole> GetAllAsync()
  {
    return _dbContext.UserRoles.ToList();
  }

  public IEnumerable<UserRole> GetAllByUserId(Guid userId)
  {
    return _dbContext.UserRoles
      .Include(u => u.Role)
      .ThenInclude(r => r.Permissions)
      .Where(ur => ur.UserId == userId).ToList();
  }

  public UserRole? GetUserRole(Guid userId, Guid roleId, string? museumId)
  {
    return _dbContext.UserRoles
      .FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId && ur.MuseumId == museumId);
  }

  public bool IsSuperAdmin(Guid userId)
  {
    return _dbContext.UserRoles.Any(ur => ur.UserId == userId && ur.Role.Name == UserConst.ROLE_SUPERADMIN);
  }
}