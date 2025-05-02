using System.Text.Json;

namespace Application.DTOs.User;

public class UserUpdateDto
{
  public string? FullName { get; set; }
  public string? Email { get; set; }
  public string? PhoneNumber { get; set; }
  public string? AvatarUrl { get; set; }
  public DateTime? BirthDate { get; set; }
  public JsonDocument? Metadata { get; set; }
}

public class ChangePassword
{
  public string OldPassword { get; set; } = null!;
  public string NewPassword { get; set; } = null!;
}