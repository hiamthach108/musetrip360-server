namespace Core.Jwt;

using Application.Shared.Enum;

public class Payload
{
  public Guid UserId { get; set; }
  public string Email { get; set; } = null!;
  public Guid SessionId { get; set; }
  public UserStatusEnum Status { get; set; }
  public bool IsAdmin { get; set; }
}
