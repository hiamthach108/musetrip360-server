namespace Domain.Rolebase;

using Application.Shared.Type;
using Domain.Users;

public class Role : BaseEntity
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public bool IsActive { get; set; } = true;

  // many to many with Permission
  public ICollection<Permission> Permissions { get; set; } = new List<Permission>();

  // many to many with User
  public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
