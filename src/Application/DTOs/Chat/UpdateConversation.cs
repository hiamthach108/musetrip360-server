namespace Application.DTOs.Chat;

using System.Text.Json;

public class UpdateConversation
{
  public string? Name { get; set; }
  public JsonDocument? Metadata { get; set; }
}