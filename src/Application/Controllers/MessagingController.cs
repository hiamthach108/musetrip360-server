namespace Application.Controllers;

using Application.DTOs.Chat;
using Application.DTOs.Notification;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/messaging")]
public class MessagingController : ControllerBase
{
  private readonly ILogger<MessagingController> _logger;
  private readonly IMessagingService _service;

  public MessagingController(ILogger<MessagingController> logger, IMessagingService service)
  {
    _logger = logger;
    _service = service;
  }

  [Protected]
  [HttpGet("conversations")]
  public async Task<IActionResult> GetConversations()
  {
    _logger.LogInformation("Get conversation users request received");
    return await _service.HandleGetConversationUsers();
  }

  [Protected]
  [HttpGet("conversations/{conversationId}/messages")]
  public async Task<IActionResult> GetConversationMessages(Guid conversationId, [FromQuery] GetConversationParams req)
  {
    _logger.LogInformation("Get conversation messages request received");
    req.ConversationId = conversationId;
    return await _service.HandleGetConversationMessages(req);
  }

  [Protected]
  [HttpPost("conversations")]
  public async Task<IActionResult> CreateConversation([FromBody] CreateConversation req)
  {
    _logger.LogInformation("Create conversation request received");
    return await _service.HandleCreateConversation(req);
  }

  [Protected]
  [HttpPost("messages")]
  public async Task<IActionResult> CreateMessage([FromBody] CreateMessage req)
  {
    _logger.LogInformation("Create message request received");
    return await _service.HandleCreateMessage(req);
  }

  [Protected]
  [HttpPut("conversations/{conversationId}/last-seen")]
  public async Task<IActionResult> UpdateLastSeen(Guid conversationId)
  {
    _logger.LogInformation("Update last seen request received");
    return await _service.HandleUpdateLastSeen(conversationId);
  }

  [Protected]
  [HttpPost("conversations/{conversationId}/join")]
  public async Task<IActionResult> JoinConversation(Guid conversationId)
  {
    _logger.LogInformation("Join conversation request received");
    return await _service.HandleJoinConversation(conversationId);
  }

  [Protected]
  [HttpPost("notifications/system")]
  public async Task<IActionResult> CreateSystemNotification([FromBody] CreateNotificationDto req)
  {
    _logger.LogInformation("Create notification request received");
    return await _service.HandleCreateSystemNotification(req);
  }

  [Protected]
  [HttpGet("notifications")]
  public async Task<IActionResult> GetNotifications([FromQuery] NotificationQuery query)
  {
    _logger.LogInformation("Get notifications request received");
    return await _service.HandleGetUserNotification(query);
  }

  [Protected]
  [HttpPut("notifications/read")]
  public async Task<IActionResult> UpdateNotificationReadStatus([FromBody] NotificationUpdateReadStatusReq req)
  {
    _logger.LogInformation("Update notification read status request received");
    return await _service.HandleUpdateNotificationReadStatus(req);
  }

  [Protected]
  [HttpPost("notifications/test")]
  public async Task<IActionResult> TestNotification([FromBody] CreateNotificationDto req)
  {
    _logger.LogInformation("Test notification request received");

    await _service.PushNewNotification(new CreateNotificationDto
    {
      Title = req.Title,
      Message = req.Message,
      Type = req.Type,
      UserId = req.UserId,
      Metadata = req.Metadata
    });

    return Ok(new
    {
      message = "Test notification sent successfully"
    });
  }

  [Protected]
  [HttpDelete("notifications/{notificationId}")]
  public async Task<IActionResult> DeleteNotification(Guid notificationId)
  {
    _logger.LogInformation("Delete notification request received");
    return await _service.HandleDeleteNotification(notificationId);
  }

  [Protected]
  [HttpPut("conversations/{conversationId}")]
  public async Task<IActionResult> UpdateConversation(Guid conversationId, [FromBody] UpdateConversation req)
  {
    _logger.LogInformation("Update conversation request received");
    return await _service.HandleUpdateConversation(conversationId, req);
  }

  [Protected]
  [HttpDelete("conversations/{conversationId}")]
  public async Task<IActionResult> DeleteConversation(Guid conversationId)
  {
    _logger.LogInformation("Delete conversation request received");
    return await _service.HandleDeleteConversation(conversationId);
  }
}

