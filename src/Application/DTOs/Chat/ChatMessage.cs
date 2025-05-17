namespace Application.DTOs.Chat;

using System.Text.Json;

public class CreateMessage
{
  public Guid ConversationId { get; set; }
  public string Content { get; set; } = null!;
  public JsonDocument? Metadata { get; set; }
}

public class MultipleMessages
{
  public Guid ConversationId { get; set; }
  public IEnumerable<MessageBase> Messages { get; set; } = [];
}

public class MessageBase
{
  public string Content { get; set; } = null!;
  public JsonDocument? Metadata { get; set; }
}