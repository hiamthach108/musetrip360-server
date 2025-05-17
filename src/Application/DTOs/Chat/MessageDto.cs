namespace Application.DTOs.Chat;

using Application.DTOs.User;
using AutoMapper;
using Domain.Messaging;

public class MessageDto
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public string Content { get; set; } = null!;
  public Guid ConversationId { get; set; }
  public Guid CreatedBy { get; set; }

  public UserBlobDto CreatedUser { get; set; } = null!;
}

public class MessageProfile : Profile
{
  public MessageProfile()
  {
    CreateMap<Message, MessageDto>();
  }
}