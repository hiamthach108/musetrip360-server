namespace Application.Controllers;

using Application.DTOs.Chat;
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
}

