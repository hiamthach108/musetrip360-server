namespace Application.Service;

using Application.DTOs.Chat;
using Application.Shared.Type;
using AutoMapper;
using Core.Realtime;
using Database;
using Domain.Messaging;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;


public interface IMessagingService
{
  Task<IActionResult> HandleGetConversationUsers();
  Task<IActionResult> HandleGetConversationMessages(GetConversationParams req);
  Task<IActionResult> HandleCreateConversation(CreateConversation req);
  Task<IActionResult> HandleCreateMessage(CreateMessage req);
  Task<IActionResult> HandleUpdateLastSeen(Guid conversationId);
  Task<IActionResult> HandleJoinConversation(Guid converstationId);
}

public class MessagingService : BaseService, IMessagingService
{
  private readonly IConversationRepository _conversationRepo;
  private readonly IMessageRepository _messageRepo;
  private readonly IRealtimeService _realtimeSvc;

  public MessagingService(
    MuseTrip360DbContext dbContext,
    IMapper mapper,
    IHttpContextAccessor httpCtx,
    IRealtimeService realtimeService
  ) : base(dbContext, mapper, httpCtx)
  {
    _conversationRepo = new ConversationRepository(dbContext);
    _messageRepo = new MessageRepository(dbContext);
    _realtimeSvc = realtimeService;
  }

  public async Task<IActionResult> HandleCreateConversation(CreateConversation req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var existConversation = await _conversationRepo.GetConversationByUsers(payload.UserId, req.ChatWithUserId);
    if (existConversation != null)
    {
      // update conversation name
      if (!string.IsNullOrEmpty(req.Name))
      {
        existConversation.Name = req.Name;
        await _conversationRepo.UpdateName(existConversation.Id, req.Name);
      }

      var conversationDto = _mapper.Map<ConversationDto>(existConversation);
      return SuccessResp.Ok(conversationDto);
    }

    var conversation = new Conversation
    {
      Name = req.Name,
      IsBot = req.IsBot,
      CreatedBy = payload.UserId,
      Metadata = req.Metadata
    };

    var result = await _conversationRepo.CreateConversation(conversation);
    if (result == null)
    {
      return ErrorResp.InternalServerError("Failed to create conversation");
    }

    var userIds = new List<Guid>
    {
        payload.UserId
    };
    if (req.ChatWithUserId != Guid.Empty)
    {
      userIds.Add(req.ChatWithUserId);
    }

    await _conversationRepo.AddUsersToConversation(result.Id, userIds);

    var con = _mapper.Map<ConversationDto>(result);

    return SuccessResp.Ok(con);
  }

  public async Task<IActionResult> HandleCreateMessage(CreateMessage req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var userConversation = _conversationRepo.GetConversationUser(req.ConversationId, payload.UserId);
    if (userConversation == null)
    {
      return ErrorResp.NotFound("Conversation not found");
    }

    var message = new Message
    {
      ConversationId = req.ConversationId,
      CreatedBy = payload.UserId,
      Content = req.Content,
      Metadata = req.Metadata
    };

    // var messages = req.Messages.Select(m => new Message
    // {
    //   ConversationId = req.ConversationId,
    //   CreatedBy = payload.UserId,
    //   Content = m.Content,
    //   Metadata = m.Metadata
    // });

    var result = await _messageRepo.CreateMessage(message);

    var msg = _mapper.Map<MessageDto>(result);

    await _conversationRepo.UpdateLastMessage(req.ConversationId, msg.Id);

    // run in background
    var users = await _conversationRepo.GetConversationUserIds(req.ConversationId);
    if (users != null && users.Count > 0)
    {
      var userIds = users.Select(u => u.ToString()).ToList();
      await _realtimeSvc.SendMessage(msg, userIds);
    }

    // var listMsg = _mapper.Map<IEnumerable<MessageDto>(result);

    return SuccessResp.Ok(msg);
  }

  public async Task<IActionResult> HandleGetConversationMessages(GetConversationParams req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var userConversation = _conversationRepo.GetConversationUser(req.ConversationId, payload.UserId);
    if (userConversation == null)
    {
      return ErrorResp.NotFound("Conversation not found");
    }

    var messages = _messageRepo.GetConversationMessages(req.ConversationId, req.Page, req.PageSize);

    var dtos = _mapper.Map<IEnumerable<MessageDto>>(messages.Messages);

    return SuccessResp.Ok(new
    {
      messages = dtos,
      total = messages.Total
    });
  }

  public async Task<IActionResult> HandleGetConversationUsers()
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var conversations = _conversationRepo.GetConversationUsers(payload.UserId);

    if (conversations == null)
    {
      return ErrorResp.NotFound("Conversations not found");
    }
    var data = _mapper.Map<IEnumerable<ConversationUserDto>>(conversations);

    return SuccessResp.Ok(
      new { conversations = data }
    );
  }

  public async Task<IActionResult> HandleJoinConversation(Guid converstationId)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var userConversation = _conversationRepo.GetConversationUser(converstationId, payload.UserId);
    if (userConversation == null)
    {
      await _conversationRepo.AddUsersToConversation(converstationId, new List<Guid> { payload.UserId });

      return SuccessResp.Ok(
        new { message = "Joined conversation" }
      );
    }

    return SuccessResp.Ok(
      new { message = "Joined conversation" }
    );
  }

  public async Task<IActionResult> HandleUpdateLastSeen(Guid conversationId)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var userConversation = _conversationRepo.GetConversationUser(conversationId, payload.UserId);
    if (userConversation == null)
    {
      return ErrorResp.NotFound("Conversation not found");
    }

    await _conversationRepo.UpdateLastSeen(conversationId, payload.UserId);

    return SuccessResp.Ok(
      new { message = "Last seen updated" }
    );
  }
}
