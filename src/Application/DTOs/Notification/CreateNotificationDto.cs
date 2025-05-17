namespace Application.DTOs.Notification;

using System.Text.Json;
using Application.Shared.Enum;

public class CreateNotificationDto
{
  public string Title { get; set; } = null!;
  public string Message { get; set; } = null!;
  public string Type { get; set; } = null!;
  public Guid UserId { get; set; }
  public NotificationTargetEnum Target { get; set; }
  public JsonDocument? Metadata { get; set; }
}
