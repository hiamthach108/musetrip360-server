namespace Application.DTOs.Notification;

using System.Text.Json;

public class CreateNotificationDto
{
  public string Title { get; set; } = null!;
  public string Message { get; set; } = null!;
  public string Type { get; set; } = null!;
  public JsonDocument? Metadata { get; set; }
}
