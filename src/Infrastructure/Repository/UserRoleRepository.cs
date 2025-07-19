namespace Infrastructure.Repository;

using Application.DTOs.UserRole;
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
  UserRoleList GetListByMuseumId(UserRoleQuery query);
  bool IsSuperAdmin(Guid userId);
}

public class UserRoleList
{
  public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
  public int Total { get; set; }
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

  public UserRoleList GetListByMuseumId(UserRoleQuery q)
  {
    var query = _dbContext.UserRoles
      .Include(ur => ur.Role)
      .Include(ur => ur.User)
      .Where(ur => ur.MuseumId == q.MuseumId)
      .AsQueryable();

    if (!string.IsNullOrEmpty(q.Search))
    {
      query = query.Where(ur => ur.User.FullName.Contains(q.Search) || ur.User.Email.Contains(q.Search));
    }

    if (q.Statuses.Count > 0)
    {
      query = query.Where(ur => q.Statuses.Contains(ur.User.Status));
    }

    var total = query.Count();
    var userRoles = query.Skip((q.Page - 1) * q.PageSize)
      .Take(q.PageSize)
      .ToList();

    return new UserRoleList { UserRoles = userRoles, Total = total };
  }
}