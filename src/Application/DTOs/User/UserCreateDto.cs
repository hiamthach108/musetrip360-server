namespace Application.DTOs.User;

public class UserCreateDto
{
  public string FullName { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string? Phone { get; set; }
  public string? Status { get; set; }
}