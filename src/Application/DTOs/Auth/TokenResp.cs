

namespace Application.DTOs.Auth;

public class TokenResp
{
  public string? access_token { get; set; }
  public int access_token_exp { get; set; }
}