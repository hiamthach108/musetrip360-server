namespace MuseTrip360.Controllers;

using Application.DTOs.Plan;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/subscriptions")]
public class SubscriptionController : ControllerBase
{
  private readonly ILogger<SubscriptionController> _logger;
  private readonly ISubscriptionService _subscriptionService;

  public SubscriptionController(ILogger<SubscriptionController> logger, ISubscriptionService subscriptionService)
  {
    _logger = logger;
    _subscriptionService = subscriptionService;
  }

  [HttpGet("plans")]
  public async Task<IActionResult> GetAllPlans([FromQuery] PlanQuery query)
  {
    _logger.LogInformation("Getting all plans with query: {@Query}", query);
    return await _subscriptionService.HandleGetAllAsync(query);
  }

  [HttpGet("plans/admin")]
  [Protected]
  public async Task<IActionResult> GetAdminPlans()
  {
    _logger.LogInformation("Getting admin plans");
    return await _subscriptionService.HandleGetAdminAsync();
  }

  [HttpGet("plans/{id}")]
  public async Task<IActionResult> GetPlanById(Guid id)
  {
    _logger.LogInformation("Getting plan by ID: {PlanId}", id);
    return await _subscriptionService.HandleGetByIdAsync(id);
  }

  [HttpPost("plans")]
  [Protected]
  public async Task<IActionResult> CreatePlan([FromBody] PlanCreateDto dto)
  {
    _logger.LogInformation("Creating new plan: {@Plan}", dto);
    return await _subscriptionService.HandleCreateAsync(dto);
  }

  [HttpPut("plans/{id}")]
  [Protected]
  public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] PlanUpdateDto dto)
  {
    _logger.LogInformation("Updating plan {PlanId}: {@Plan}", id, dto);
    return await _subscriptionService.HandleUpdateAsync(id, dto);
  }

  [HttpDelete("plans/{id}")]
  [Protected]
  public async Task<IActionResult> DeletePlan(Guid id)
  {
    _logger.LogInformation("Deleting plan: {PlanId}", id);
    return await _subscriptionService.HandleDeleteAsync(id);
  }
}