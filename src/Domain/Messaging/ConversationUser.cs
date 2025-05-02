namespace Domain.Messaging;

using Application.Shared.Type;
using Domain.Users;

public class ConversationUser : BaseEntity
{
  public Guid ConversationId { get; set; }
  public Guid UserId { get; set; }
  public Guid LastMessageId { get; set; }
  public DateTime LastMessageAt { get; set; }

  public Conversation Conversation { get; set; } = null!;
  public User User { get; set; } = null!;
  public Message LastMessage { get; set; } = null!;
}
