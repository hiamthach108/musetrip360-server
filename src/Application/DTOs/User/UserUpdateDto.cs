namespace Application.DTOs.User;

public class UserUpdateDto
{
  public string? FullName { get; set; }
  public string? Email { get; set; }
  public string? Phone { get; set; }
}

public class ChangePassword
{
  public string OldPassword { get; set; } = null!;
  public string NewPassword { get; set; } = null!;
}