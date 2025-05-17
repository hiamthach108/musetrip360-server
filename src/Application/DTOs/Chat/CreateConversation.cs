namespace Application.DTOs.Chat;

using System.Text.Json;

public class CreateConversation
{
  public string? Name { get; set; }
  public bool IsBot { get; set; }
  public JsonDocument? Metadata { get; set; }

  public Guid ChatWithUserId { get; set; }
}