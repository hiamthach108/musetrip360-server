using Application.DTOs.User;
using Application.Shared.Type;

namespace Application.DTOs.Auth;

public class RegisterReq
{
  public string Email { get; set; } = null!;
  public string Password { get; set; } = null!;
  public string FullName { get; set; } = null!;
  public string? PhoneNumber { get; set; }
  public string? AvatarUrl { get; set; }
}

public class RegisterResp : BaseResp
{
  public UserDto User { get; set; } = null!;
}