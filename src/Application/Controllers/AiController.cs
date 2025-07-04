namespace Application.Controllers;

using Application.DTOs.Ai;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/v1/ai")]
public class AiController : ControllerBase
{
  private readonly IAiService _aiService;
  private readonly ILogger<AiController> _logger;

  public AiController(IAiService aiService, ILogger<AiController> logger)
  {
    _aiService = aiService;
    _logger = logger;
  }

  [Protected]
  [HttpPost("chat")]
  public async Task<IActionResult> Chat([FromBody] ChatReq req)
  {
    _logger.LogInformation("Chat request received");
    return await _aiService.HandleChat(req);
  }

  [Protected]
  [HttpPost("embedding")]
  public async Task<IActionResult> Embedding([FromBody] ChatReq req)
  {
    _logger.LogInformation("Embedding request received");
    return await _aiService.HandleEmbedding(req);
  }
}