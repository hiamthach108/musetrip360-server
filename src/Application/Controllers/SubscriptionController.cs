namespace MuseTrip360.Controllers;

using Application.DTOs.Plan;
using Application.DTOs.Subscription;
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

  [HttpPost("buy")]
  [Protected]
  public async Task<IActionResult> BuySubscription([FromBody] BuySubscriptionDto dto)
  {
    _logger.LogInformation("User buying subscription: {@SubscriptionDto}", dto);
    return await _subscriptionService.HandleBuySubscriptionAsync(dto);
  }

  [HttpGet("my")]
  [Protected]
  public async Task<IActionResult> GetMySubscriptions([FromQuery] SubscriptionQuery query)
  {
    _logger.LogInformation("Getting user subscriptions with query: {@Query}", query);
    return await _subscriptionService.HandleGetUserSubscriptionsAsync(query);
  }

  [HttpGet("{id}")]
  [Protected]
  public async Task<IActionResult> GetSubscriptionById(Guid id)
  {
    _logger.LogInformation("Getting subscription by ID: {SubscriptionId}", id);
    return await _subscriptionService.HandleGetSubscriptionByIdAsync(id);
  }

  [HttpGet("active/museum/{museumId}")]
  [Protected]
  public async Task<IActionResult> GetActiveSubscriptionForMuseum(Guid museumId)
  {
    _logger.LogInformation("Getting active subscription for museum: {MuseumId}", museumId);
    return await _subscriptionService.HandleGetActiveSubscriptionAsync(museumId);
  }

  [HttpPut("{id}/cancel")]
  [Protected]
  public async Task<IActionResult> CancelSubscription(Guid id)
  {
    _logger.LogInformation("Cancelling subscription: {SubscriptionId}", id);
    return await _subscriptionService.HandleCancelSubscriptionAsync(id);
  }

  [HttpGet("museum/{museumId}")]
  [Protected]
  public async Task<IActionResult> GetMuseumSubscriptions(Guid museumId)
  {
    _logger.LogInformation("Getting subscriptions for museum: {MuseumId}", museumId);
    return await _subscriptionService.HandleGetMuseumSubscriptionsAsync(museumId);
  }

  // Plan Management Endpoints
  [HttpGet("plans")]
  public async Task<IActionResult> GetAllPlans([FromQuery] PlanQuery query)
  {
    _logger.LogInformation("Getting all plans with query: {@Query}", query);
    return await _subscriptionService.HandleGetAllPlansAsync(query);
  }

  [HttpGet("plans/admin")]
  [Protected]
  public async Task<IActionResult> GetAdminPlans()
  {
    _logger.LogInformation("Getting admin plans");
    return await _subscriptionService.HandleGetAdminPlansAsync();
  }

  [HttpGet("plans/{id}")]
  public async Task<IActionResult> GetPlanById(Guid id)
  {
    _logger.LogInformation("Getting plan by ID: {PlanId}", id);
    return await _subscriptionService.HandleGetPlanByIdAsync(id);
  }

  [HttpPost("plans")]
  [Protected]
  public async Task<IActionResult> CreatePlan([FromBody] PlanCreateDto dto)
  {
    _logger.LogInformation("Creating new plan: {@Plan}", dto);
    return await _subscriptionService.HandleCreatePlanAsync(dto);
  }

  [HttpPut("plans/{id}")]
  [Protected]
  public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] PlanUpdateDto dto)
  {
    _logger.LogInformation("Updating plan {PlanId}: {@Plan}", id, dto);
    return await _subscriptionService.HandleUpdatePlanAsync(id, dto);
  }

  [HttpDelete("plans/{id}")]
  [Protected]
  public async Task<IActionResult> DeletePlan(Guid id)
  {
    _logger.LogInformation("Deleting plan: {PlanId}", id);
    return await _subscriptionService.HandleDeletePlanAsync(id);
  }
}