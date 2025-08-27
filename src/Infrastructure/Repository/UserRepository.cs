namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.User;
using Application.Shared.Constant;
using Database;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

public interface IUserRepository
{
  Task<User> GetByIdAsync(Guid id);
  Task<IEnumerable<User>> GetAllAsync();
  Task<UserList> GetUserListAsync(UserQuery query);
  Task<User> AddAsync(User user);
  Task<User> UpdateAsync(Guid userId, User user);
  Task<User> DeleteAsync(User user);
  Task<User> GetUserByEmail(string email);
  Task<List<string>> GetMuseumManagerEmailsByMuseumId(string museumId);
}

public class UserList
{
  public IEnumerable<User> Users { get; set; } = [];
  public int Total { get; set; }
}

public class UserRepository : IUserRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public UserRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<User> AddAsync(User user)
  {

    // Add user to database
    var result = await _dbContext.Users.AddAsync(user);

    // Save changes
    await _dbContext.SaveChangesAsync();

    return result.Entity;
  }

  public async Task<User> DeleteAsync(User user)
  {

    // Remove user from database
    var result = _dbContext.Users.Remove(user);

    // Save changes
    await _dbContext.SaveChangesAsync();

    return result.Entity;
  }

  public async Task<IEnumerable<User>> GetAllAsync()
  {
    // Get all users from database
    var users = _dbContext.Users.AsEnumerable();

    return users;
  }

  public async Task<UserList> GetUserListAsync(UserQuery query)
  {
    string searchKeyword = query.Search ?? "";
    int page = query.Page < 1 ? 1 : query.Page;
    int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

    var q = _dbContext.Users
      .Where(u => u.Email.ToLower().Contains(searchKeyword.ToLower()) || u.FullName.ToLower().Contains(searchKeyword.ToLower()))
      .AsQueryable();

    var total = q.Count();

    var users = q
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToList();

    return new UserList { Users = users, Total = total };
  }

  public async Task<User> GetByIdAsync(Guid id)
  {
    var user = await _dbContext.Users.FindAsync(id);

    return user;
  }

  public async Task<User?> UpdateAsync(Guid userId, User user)
  {

    // Update user in database
    var u = await _dbContext.Users.FindAsync(userId);

    if (u == null) return null;

    var result = _dbContext.Users.Update(user);
    // Save changes
    await _dbContext.SaveChangesAsync();

    return result.Entity;
  }

  public async Task<User> GetUserByEmail(string email)
  {
    var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);

    return user;
  }

  public async Task<List<string>> GetMuseumManagerEmailsByMuseumId(string museumId)
  {
    var emails = await _dbContext.UserRoles
      .Include(ur => ur.User)
      .Include(ur => ur.Role)
      .Where(ur => ur.MuseumId == museumId && ur.Role.Name == UserConst.ROLE_MUSEUM_MANAGER)
      .Select(ur => ur.User.Email)
      .ToListAsync();

    return emails;
  }
}