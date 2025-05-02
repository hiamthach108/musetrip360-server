using Application.Shared.Enum;
using Application.Shared.Type;

namespace Application.DTOs.Auth;

public class LoginReq
{
  public AuthTypeEnum AuthType { get; set; }

  public string? Email { get; set; }
  public string? Password { get; set; }
  public string? Redirect { get; set; }
  public string? FirebaseToken { get; set; }
}

public class LoginGoogleResp
{
  public string RedirectLink { get; set; } = null!;
  public string Token { get; set; } = null!;
}

public class LoginEmailResp
{
  public Guid UserId { get; set; }
  public string AccessToken { get; set; } = null!;
  public string RefreshToken { get; set; } = null!;
  public long AccessTokenExpAt { get; set; }
  public long RefreshTokenExpAt { get; set; }
}

public class VerifyTokenResp
{
  public Guid UserId { get; set; }
  public string AccessToken { get; set; } = null!;
  public string RefreshToken { get; set; } = null!;
  public long AccessTokenExpAt { get; set; }
  public long RefreshTokenExpAt { get; set; }
}