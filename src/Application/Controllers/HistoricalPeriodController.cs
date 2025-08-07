namespace Application.Controllers;

using Application.DTOs.HistoricalPeriod;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/historical-periods")]
public class HistoricalPeriodController : ControllerBase
{
  private readonly ILogger<HistoricalPeriodController> _logger;
  private readonly IHistoricalPeriodService _historicalPeriodService;

  public HistoricalPeriodController(ILogger<HistoricalPeriodController> logger, IHistoricalPeriodService historicalPeriodService)
  {
    _logger = logger;
    _historicalPeriodService = historicalPeriodService;
  }

  [HttpGet]
  public async Task<IActionResult> GetAllHistoricalPeriods()
  {
    _logger.LogInformation("Get all historical periods request received");
    return await _historicalPeriodService.HandleGetAllAsync();
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetHistoricalPeriodById(Guid id)
  {
    _logger.LogInformation("Get historical period by id request received for id: {Id}", id);
    return await _historicalPeriodService.HandleGetByIdAsync(id);
  }

  [HttpGet("search")]
  public async Task<IActionResult> SearchHistoricalPeriods([FromQuery] HistoricalPeriodQueryDto query)
  {
    _logger.LogInformation("Search historical periods request received with Name: {Name}", query.Name);
    return await _historicalPeriodService.HandleSearchAsync(query);
  }

  [Protected]
  [HttpPost]
  public async Task<IActionResult> CreateHistoricalPeriod([FromBody] HistoricalPeriodCreateDto dto)
  {
    _logger.LogInformation("Create historical period request received");
    return await _historicalPeriodService.HandleCreateAsync(dto);
  }

  [Protected]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateHistoricalPeriod(Guid id, [FromBody] HistoricalPeriodUpdateDto dto)
  {
    _logger.LogInformation("Update historical period request received for id: {Id}", id);
    return await _historicalPeriodService.HandleUpdateAsync(id, dto);
  }

  [Protected]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteHistoricalPeriod(Guid id)
  {
    _logger.LogInformation("Delete historical period request received for id: {Id}", id);
    return await _historicalPeriodService.HandleDeleteAsync(id);
  }
}