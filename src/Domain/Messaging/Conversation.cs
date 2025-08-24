namespace Domain.Messaging;

using Application.Shared.Type;
using Domain.Users;

public class Conversation : BaseEntity
{
  public string? Name { get; set; }
  public bool IsBot { get; set; }
  public Guid CreatedBy { get; set; }


  public User CreatedByUser { get; set; } = null!;
  public ICollection<ConversationUser> ConversationUsers { get; set; } = new List<ConversationUser>();
}