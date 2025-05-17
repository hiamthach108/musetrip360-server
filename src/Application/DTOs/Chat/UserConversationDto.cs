namespace Application.DTOs.Chat;

using Application.DTOs.User;
using AutoMapper;
using Domain.Messaging;

public class ConversationUserDto
{
  public Guid UserId { get; set; }
  public Guid ConversationId { get; set; }
  public DateTime LastMessageReadAt { get; set; }
  public UserBlobDto User { get; set; } = null!;
  public ConversationDto Conversation { get; set; } = null!;
}

public class ConversationUserProfile : Profile
{
  public ConversationUserProfile()
  {
    CreateMap<ConversationUser, ConversationUserDto>();
  }
}