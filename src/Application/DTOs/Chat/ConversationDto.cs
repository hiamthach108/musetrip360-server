namespace Application.DTOs.Chat;

using System.Text.Json;
using Application.DTOs.User;
using AutoMapper;
using Domain.Messaging;
using Newtonsoft.Json;

public class ConversationDto
{
  public Guid Id { get; set; }
  public string? Name { get; set; }
  public bool IsBot { get; set; }

  public JsonDocument? Metadata { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public UserBlobDto CreatedUser { get; set; } = null!;
  public MessageDto? LastMessage { get; set; } = null!;
}

public class ConversationProfile : Profile
{

  public ConversationProfile()
  {
    CreateMap<Conversation, ConversationDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<ConversationDto, Conversation>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}