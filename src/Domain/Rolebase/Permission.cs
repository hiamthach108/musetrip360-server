namespace Domain.Rolebase;

using Application.Shared.Type;

public class Permission : BaseEntity
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public string? ResourceGroup { get; set; }
  public bool IsActive { get; set; } = true;

  // many to many with Role
  public ICollection<Role> Roles { get; set; } = new List<Role>();
}