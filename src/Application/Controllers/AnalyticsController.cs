namespace Application.Controllers;

using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/v1/analytics")]
public class AnalyticsController : ControllerBase
{
  private readonly IAnalyticsService _analyticsService;
  private readonly ILogger<AnalyticsController> _logger;

  public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
  {
    _analyticsService = analyticsService;
    _logger = logger;
  }

  [Protected]
  [HttpGet("overview/{museumId}")]
  public async Task<IActionResult> GetOverview(Guid museumId)
  {
    _logger.LogInformation("GetOverview request received for museumId: {MuseumId}", museumId);
    return await _analyticsService.GetOverview(museumId);
  }
}