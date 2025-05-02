namespace Application.DTOs.Auth;


// userId = user.Id, sessionId, refresh = refreshTk, role = user.RoleId
public class RedisSession
{
  public Guid UserId { get; set; }
  public Guid SessionId { get; set; }
  public string? Refresh { get; set; }
}

public class LogoutReq
{
  public required string RefreshToken { get; set; }
}
