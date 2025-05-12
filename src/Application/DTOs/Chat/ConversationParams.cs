namespace Application.DTOs.Chat;

public class GetConversationParams
{
  public Guid ConversationId { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
}