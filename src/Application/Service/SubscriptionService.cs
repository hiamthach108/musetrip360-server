namespace Application.Service;

using Application.DTOs.Subscription;
using Application.DTOs.Plan;
using Application.Shared.Type;
using Application.Shared.Enum;
using Application.Shared.Helpers;
using AutoMapper;
using Domain.Subscription;
using Domain.Payment;
using Database;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Core.Payos;
using Core.Queue;
using Net.payOS.Types;
using System.Text.Json;

public interface ISubscriptionService
{
  // Subscription Management
  Task<IActionResult> HandleBuySubscriptionAsync(BuySubscriptionDto dto);
  Task<IActionResult> HandleGetUserSubscriptionsAsync(SubscriptionQuery query);
  Task<IActionResult> HandleGetSubscriptionByIdAsync(Guid id);
  Task<IActionResult> HandleGetActiveSubscriptionAsync(Guid museumId);
  Task<IActionResult> HandleCancelSubscriptionAsync(Guid id);
  Task<IActionResult> HandleGetMuseumSubscriptionsAsync(Guid museumId);
  Task<IActionResult> HandleGetAllSubscriptionsAsync(SubscriptionQuery query);

  // Plan Management
  Task<IActionResult> HandleGetAllPlansAsync(PlanQuery query);
  Task<IActionResult> HandleGetPlanByIdAsync(Guid id);
  Task<IActionResult> HandleCreatePlanAsync(PlanCreateDto dto);
  Task<IActionResult> HandleUpdatePlanAsync(Guid id, PlanUpdateDto dto);
  Task<IActionResult> HandleDeletePlanAsync(Guid id);
  Task<IActionResult> HandleGetAdminPlansAsync();
}

public class SubscriptionService : BaseService, ISubscriptionService
{
  private readonly ISubscriptionRepository _subscriptionRepository;
  private readonly IPlanRepository _planRepository;
  private readonly IOrderRepository _orderRepository;
  private readonly IPaymentRepository _paymentRepository;
  private readonly IMuseumRepository _museumRepository;
  private readonly IPayOSService _payOSService;
  private readonly IQueuePublisher _queuePublisher;
  private readonly ILogger<SubscriptionService> _logger;

  public SubscriptionService(
      MuseTrip360DbContext dbContext,
      IMapper mapper,
      IHttpContextAccessor httpCtx,
      IPayOSService payOSService,
      IQueuePublisher queuePublisher,
      ILogger<SubscriptionService> logger)
      : base(dbContext, mapper, httpCtx)
  {
    _subscriptionRepository = new SubscriptionRepository(dbContext);
    _planRepository = new PlanRepository(dbContext);
    _orderRepository = new OrderRepository(dbContext);
    _paymentRepository = new PaymentRepository(dbContext);
    _museumRepository = new MuseumRepository(dbContext);
    _payOSService = payOSService;
    _queuePublisher = queuePublisher;
    _logger = logger;
  }

  public async Task<IActionResult> HandleBuySubscriptionAsync(BuySubscriptionDto dto)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      // Check if plan exists and is active
      var plan = await _planRepository.GetByIdAsync(dto.PlanId);
      if (plan == null || !plan.IsActive)
      {
        return ErrorResp.NotFound("Plan not found or inactive");
      }

      // Check if museum exists
      var museum = await _museumRepository.GetByIdAsync(dto.MuseumId);
      if (museum == null)
      {
        return ErrorResp.NotFound("Museum not found");
      }

      // Check if user already has active subscription for this museum
      var existingSubscription = await _subscriptionRepository.GetActiveSubscriptionByMuseumIdAsync(dto.MuseumId);
      if (existingSubscription != null)
      {
        return ErrorResp.BadRequest("You already have an active subscription for this museum");
      }

      // Create order
      var snowflake = new SnowflakeId(1);
      var orderCode = snowflake.GenerateOrderId();
      var totalAmount = (int)plan.Price;

      // Create PayOS payment
      var paymentData = new PaymentData(
        orderCode: orderCode,
        amount: totalAmount,
        description: $"{orderCode}",
        items: [new ItemData(plan.Id.ToString(), 1, totalAmount)],
        cancelUrl: dto.CancelUrl ?? "",
        returnUrl: dto.SuccessUrl ?? ""
      );

      var paymentResult = await _payOSService.CreatePayment(paymentData);

      // Create order in database
      var order = new Order
      {
        CreatedBy = payload.UserId,
        TotalAmount = totalAmount,
        OrderCode = paymentResult.orderCode.ToString(),
        ExpiredAt = paymentResult.expiredAt.HasValue ?
          DateTime.UnixEpoch.AddSeconds(paymentResult.expiredAt.Value) :
          DateTime.UtcNow.AddDays(1),
        Status = PaymentStatusEnum.Pending,
        OrderType = OrderTypeEnum.Subscription,
        Metadata = JsonDocument.Parse(JsonSerializer.Serialize(paymentResult))
      };

      var createdOrder = await _orderRepository.AddAsync(order);

      // Create pending subscription
      var subscription = new Subscription
      {
        UserId = payload.UserId,
        PlanId = dto.PlanId,
        OrderId = createdOrder.Id,
        MuseumId = dto.MuseumId,
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
        Status = SubscriptionStatusEnum.Cancelled, // Will be activated when payment is successful
        Metadata = dto.Metadata
      };

      await _subscriptionRepository.AddAsync(subscription);

      return SuccessResp.Ok(new
      {
        paymentResult.checkoutUrl,
        paymentResult.orderCode,
        paymentResult.expiredAt,
        paymentResult.paymentLinkId,
        paymentResult.status,
        paymentResult.currency,
        paymentResult.amount,
        paymentResult.description,
        paymentResult.bin,
        paymentResult.accountNumber,
        paymentResult.qrCode,
        CancelUrl = dto.CancelUrl ?? "",
        ReturnUrl = dto.SuccessUrl ?? ""
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error buying subscription");
      return ErrorResp.InternalServerError($"Error processing subscription purchase: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetUserSubscriptionsAsync(SubscriptionQuery query)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var subscriptions = await _subscriptionRepository.GetByUserIdWithDetailsAsync(payload.UserId);
      var subscriptionDtos = _mapper.Map<List<SubscriptionSummaryDto>>(subscriptions);

      return SuccessResp.Ok(subscriptionDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving subscriptions: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetSubscriptionByIdAsync(Guid id)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(id);
      if (subscription == null)
      {
        return ErrorResp.NotFound("Subscription not found");
      }

      // Only allow users to view their own subscriptions
      if (subscription.UserId != payload.UserId)
      {
        return ErrorResp.Forbidden("You can only view your own subscriptions");
      }

      var subscriptionDto = _mapper.Map<SubscriptionDto>(subscription);
      return SuccessResp.Ok(subscriptionDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving subscription: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetActiveSubscriptionAsync(Guid museumId)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var subscription = await _subscriptionRepository.GetActiveSubscriptionByMuseumIdAsync(museumId);
      if (subscription == null)
      {
        return ErrorResp.NotFound("No active subscription found for this museum");
      }

      var subscriptionDto = _mapper.Map<SubscriptionSummaryDto>(subscription);
      return SuccessResp.Ok(subscriptionDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving active subscription: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleCancelSubscriptionAsync(Guid id)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var subscription = await _subscriptionRepository.GetByIdAsync(id);
      if (subscription == null)
      {
        return ErrorResp.NotFound("Subscription not found");
      }

      // Only allow users to cancel their own subscriptions
      if (subscription.UserId != payload.UserId)
      {
        return ErrorResp.Forbidden("You can only cancel your own subscriptions");
      }

      if (subscription.Status != SubscriptionStatusEnum.Active)
      {
        return ErrorResp.BadRequest("Only active subscriptions can be cancelled");
      }

      subscription.Status = SubscriptionStatusEnum.Cancelled;
      await _subscriptionRepository.UpdateAsync(id, subscription);

      return SuccessResp.Ok("Subscription cancelled successfully");
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error cancelling subscription: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetMuseumSubscriptionsAsync(Guid museumId)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var subscriptions = await _subscriptionRepository.GetByMuseumIdAsync(museumId);
      var subscriptionDtos = _mapper.Map<List<SubscriptionDto>>(subscriptions);

      return SuccessResp.Ok(subscriptionDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving museum subscriptions: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetAllSubscriptionsAsync(SubscriptionQuery query)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var resp = await _subscriptionRepository.GetAllAsync(query);
    var subscriptionDtos = _mapper.Map<List<SubscriptionDto>>(resp.Subscriptions);

    return SuccessResp.Ok(new
    {
      List = subscriptionDtos,
      Total = resp.Total
    });
  }

  // Plan Management Methods
  public async Task<IActionResult> HandleGetAllPlansAsync(PlanQuery query)
  {
    try
    {
      var plans = await _planRepository.GetAllAsync();
      var planDtos = _mapper.Map<List<PlanDto>>(plans);

      return SuccessResp.Ok(planDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving plans: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetPlanByIdAsync(Guid id)
  {
    try
    {
      var plan = await _planRepository.GetByIdAsync(id);

      if (plan == null)
      {
        return ErrorResp.NotFound("Plan not found");
      }

      var planDto = _mapper.Map<PlanDto>(plan);
      return SuccessResp.Ok(planDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving plan: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleCreatePlanAsync(PlanCreateDto dto)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      // Check if name is unique
      var isNameUnique = await _planRepository.IsNameUniqueAsync(dto.Name);
      if (!isNameUnique)
      {
        return ErrorResp.BadRequest("Plan name already exists");
      }

      var plan = _mapper.Map<Plan>(dto);
      var createdPlan = await _planRepository.AddAsync(plan);

      var planDto = _mapper.Map<PlanDto>(createdPlan);
      return SuccessResp.Created(planDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error creating plan: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleUpdatePlanAsync(Guid id, PlanUpdateDto dto)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var existingPlan = await _planRepository.GetByIdAsync(id);
      if (existingPlan == null)
      {
        return ErrorResp.NotFound("Plan not found");
      }

      // Check name uniqueness if name is being updated
      if (!string.IsNullOrEmpty(dto.Name) && dto.Name != existingPlan.Name)
      {
        var isNameUnique = await _planRepository.IsNameUniqueAsync(dto.Name, id);
        if (!isNameUnique)
        {
          return ErrorResp.BadRequest("Plan name already exists");
        }
      }

      _mapper.Map(dto, existingPlan);
      var updatedPlan = await _planRepository.UpdateAsync(existingPlan);

      var planDto = _mapper.Map<PlanDto>(updatedPlan);
      return SuccessResp.Ok(planDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error updating plan: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleDeletePlanAsync(Guid id)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var plan = await _planRepository.GetByIdAsync(id);
      if (plan == null)
      {
        return ErrorResp.NotFound("Plan not found");
      }

      // Check if plan has active subscriptions
      var hasActiveSubscriptions = plan.Subscriptions
          .Any(s => s.Status == SubscriptionStatusEnum.Active);

      if (hasActiveSubscriptions)
      {
        return ErrorResp.BadRequest("Cannot delete plan with active subscriptions");
      }

      await _planRepository.DeleteAsync(plan);
      return SuccessResp.NoContent();
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error deleting plan: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetAdminPlansAsync()
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var plans = await _planRepository.GetAllAsync();
      var planDtos = _mapper.Map<List<PlanDto>>(plans);

      return SuccessResp.Ok(planDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving admin plans: {ex.Message}");
    }
  }
}