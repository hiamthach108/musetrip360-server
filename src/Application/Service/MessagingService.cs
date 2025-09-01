namespace Application.Service;

using System.Text.Json;
using Application.DTOs.Chat;
using Application.DTOs.Notification;
using Application.DTOs.Search;
using Application.Shared.Constant;
using Application.Shared.Enum;
using Application.Shared.Type;
using AutoMapper;
using Core.LLM;
using Core.Queue;
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
  Task<IActionResult> HandleJoinConversation(Guid conversationId);
  Task<IActionResult> HandleUpdateConversation(Guid conversationId, UpdateConversation req);
  Task<IActionResult> HandleDeleteConversation(Guid conversationId);

  // Notification
  Task<IActionResult> HandleGetUserNotification(NotificationQuery query);
  Task<IActionResult> HandleUpdateNotificationReadStatus(NotificationUpdateReadStatusReq req);
  Task<IActionResult> HandleCreateSystemNotification(CreateNotificationDto req);
  Task<QueueOperationResult> PushNewNotification(CreateNotificationDto notification);
  Task<NotificationDto> HandleCreateNotification(CreateNotificationDto req);
  Task<IActionResult> HandleDeleteNotification(Guid notificationId);
}

public class MessagingService : BaseService, IMessagingService
{
  private readonly IConversationRepository _conversationRepo;
  private readonly IMessageRepository _messageRepo;
  private readonly INotificationRepository _notificationRepo;
  private readonly IRealtimeService _realtimeSvc;
  private readonly IQueuePublisher _queuePub;
  private readonly ISemanticSearchService _semanticSearchService;

  private readonly ILLM _llm;

  public MessagingService(
    MuseTrip360DbContext dbContext,
    IMapper mapper,
    IHttpContextAccessor httpCtx,
    IRealtimeService realtimeService,
    IQueuePublisher queuePublisher,
    ILLM llm,
    ISemanticSearchService semanticSearchService
  ) : base(dbContext, mapper, httpCtx)
  {
    _conversationRepo = new ConversationRepository(dbContext);
    _messageRepo = new MessageRepository(dbContext);
    _realtimeSvc = realtimeService;
    _notificationRepo = new NotificationRepository(dbContext);
    _queuePub = queuePublisher;
    _llm = llm;
    _semanticSearchService = semanticSearchService;
  }

  public async Task<IActionResult> HandleCreateConversation(CreateConversation req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
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

    var conversation = await _conversationRepo.GetByIdAsync(req.ConversationId);
    if (conversation == null)
    {
      return ErrorResp.NotFound("Conversation not found");
    }
    if (!conversation.CreatedBy.Equals(payload.UserId))
    {
      return ErrorResp.Forbidden("Cannot send message to inactive conversation");
    }

    var message = new Message
    {
      ConversationId = req.ConversationId,
      CreatedBy = payload.UserId,
      Content = req.Content,
      Metadata = req.Metadata,
      ContentType = "User"
    };

    var result = await _messageRepo.CreateMessage(message);

    var msg = _mapper.Map<MessageDto>(result);

    await _conversationRepo.UpdateLastSeen(req.ConversationId, payload.UserId);

    // Is AI process
    if (req.IsBot == true)
    {
      var semanticResult = await _semanticSearchService.SearchByQueryAsync(
        new SemanticSearchQuery
        {
          Query = req.Content,
          Page = 1,
          PageSize = 10,
          MinSimilarity = 0.7m,
          IncludeEmbeddings = false,
        });

      var resultWithData = await _llm.CompleteWithDataAsync(req.Content, [.. semanticResult.Items.Cast<object>()]);

      var aiMessage = new Message
      {
        ConversationId = req.ConversationId,
        CreatedBy = payload.UserId,
        Content = resultWithData,
        IsBot = true,
        ContentType = "AI"
      };

      // add semanticResult to metadata
      var aiMetadata = new Dictionary<string, object>
      {
        ["relatedData"] = semanticResult.Items
      };
      aiMessage.Metadata = JsonDocument.Parse(JsonSerializer.Serialize(aiMetadata));

      var aiResult = await _messageRepo.CreateMessage(aiMessage);

      if (aiResult == null)
      {
        return ErrorResp.InternalServerError("Failed to create AI message");
      }

      return SuccessResp.Ok(_mapper.Map<MessageDto>(aiResult));
    }


    return SuccessResp.Ok(msg);
  }

  public async Task<IActionResult> HandleGetConversationMessages(GetConversationParams req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var conversation = await _conversationRepo.GetByIdAsync(req.ConversationId);
    if (conversation == null)
    {
      return ErrorResp.NotFound("Conversation not found");
    }

    if (!conversation.CreatedBy.Equals(payload.UserId))
    {
      return ErrorResp.Forbidden("Cannot send message to inactive conversation");
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
    var data = _mapper.Map<IEnumerable<ConversationDto>>(conversations);

    return SuccessResp.Ok(data);
  }

  public async Task<IActionResult> HandleJoinConversation(Guid conversationId)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var userConversation = _conversationRepo.GetConversationUser(conversationId, payload.UserId);
    if (userConversation == null)
    {
      await _conversationRepo.AddUsersToConversation(conversationId, new List<Guid> { payload.UserId });

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

    return SuccessResp.Ok("Last seen updated");
  }

  public async Task<IActionResult> HandleGetUserNotification(NotificationQuery query)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var notifications = _notificationRepo.GetUserNotifications(query, payload.UserId);

    var dtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications.Notifications);

    return SuccessResp.Ok(new
    {
      list = dtos,
      total = notifications.Total,
      totalUnread = notifications.TotalUnread
    });
  }

  public async Task<IActionResult> HandleUpdateNotificationReadStatus(NotificationUpdateReadStatusReq req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var notification = await _notificationRepo.UpdateReadStatus(req.NotificationId, req.IsRead);

    return SuccessResp.Ok(_mapper.Map<NotificationDto>(notification));
  }

  public async Task<IActionResult> HandleCreateSystemNotification(CreateNotificationDto req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var notification = _mapper.Map<Notification>(req);
    notification.UserId = payload.UserId;
    notification.Target = NotificationTargetEnum.All;
    // notification.ReadAt = DateTime.MinValue;

    var result = await _notificationRepo.CreateNotification(notification);

    return SuccessResp.Ok(_mapper.Map<NotificationDto>(result));
  }

  public async Task<QueueOperationResult> PushNewNotification(CreateNotificationDto notification)
  {
    return await _queuePub.Publish(QueueConst.Notification, notification);
  }

  public async Task<NotificationDto> HandleCreateNotification(CreateNotificationDto req)
  {
    var notification = _mapper.Map<Notification>(req);

    var result = await _notificationRepo.CreateNotification(notification);

    return _mapper.Map<NotificationDto>(result);
  }

  public async Task<IActionResult> HandleUpdateConversation(Guid conversationId, UpdateConversation req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var conversation = await _conversationRepo.GetByIdAsync(conversationId);
    if (conversation == null)
    {
      return ErrorResp.NotFound("Conversation not found");
    }

    if (!conversation.CreatedBy.Equals(payload.UserId))
    {
      return ErrorResp.Forbidden("You can only update your own conversations");
    }

    var updatedConversation = await _conversationRepo.UpdateConversation(conversationId, req);
    if (updatedConversation == null)
    {
      return ErrorResp.InternalServerError("Failed to update conversation");
    }

    var conversationDto = _mapper.Map<ConversationDto>(updatedConversation);
    return SuccessResp.Ok(conversationDto);
  }

  public async Task<IActionResult> HandleDeleteConversation(Guid conversationId)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var conversation = await _conversationRepo.GetByIdAsync(conversationId);
    if (conversation == null)
    {
      return ErrorResp.NotFound("Conversation not found");
    }

    if (!conversation.CreatedBy.Equals(payload.UserId))
    {
      return ErrorResp.Forbidden("You can only delete your own conversations");
    }

    var deleted = await _conversationRepo.DeleteConversation(conversationId);
    if (!deleted)
    {
      return ErrorResp.InternalServerError("Failed to delete conversation");
    }

    return SuccessResp.Ok(new { message = "Conversation deleted successfully" });
  }

  public async Task<IActionResult> HandleDeleteNotification(Guid notificationId)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var deleted = await _notificationRepo.DeleteNotification(notificationId);
    if (!deleted)
    {
      return ErrorResp.NotFound("Notification not found");
    }

    return SuccessResp.Ok(new { message = "Notification deleted successfully" });
  }
}
