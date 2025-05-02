namespace Domain.Users;

using Application.Shared.Enum;
using Application.Shared.Type;

public class User : BaseEntity
{
  public string Username { get; set; } = null!;
  public string FullName { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string? Phone { get; set; }
  public string? HashedPassword { get; set; }
  public string? Avatar { get; set; }
  public bool IsAdmin { get; set; }
  public AuthTypeEnum AuthType { get; set; }
  public UserStatusEnum Status { get; set; }
  public DateTime LastLogin { get; set; }
}