namespace Domain.Messaging;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;

public class Notification : BaseEntity
{
  public string Title { get; set; } = null!;
  public string Message { get; set; } = null!;
  public string Type { get; set; } = null!;
  public bool IsRead { get; set; }
  public DateTime? ReadAt { get; set; }
  public NotificationTargetEnum Target { get; set; }
  public Guid UserId { get; set; }

  public User User { get; set; } = null!;
}