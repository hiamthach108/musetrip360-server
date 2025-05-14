namespace Domain.Messaging;

using Application.Shared.Type;
using Domain.Users;

public class Message : BaseEntity
{
  public string Content { get; set; } = null!;
  public Guid ConversationId { get; set; }
  public string ContentType { get; set; } = null!;
  public bool IsBot { get; set; }
  public Guid CreatedBy { get; set; }

  public Conversation Conversation { get; set; } = null!;
  public User CreatedByUser { get; set; } = null!;

  public ICollection<ConversationUser> ConversationUsers { get; set; } = new List<ConversationUser>();

  public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}