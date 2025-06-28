namespace Application.Service;

using System.Text.Json;
using Application.DTOs.Order;
using Application.DTOs.Payment;
using Application.Shared.Constant;
using Application.Shared.Enum;
using Application.Shared.Helpers;
using Application.Shared.Type;
using AutoMapper;
using Core.Queue;
using Database;
using Domain.Payment;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface IPaymentService
{
  Task<IActionResult> HandleCreateOrder(CreateOrderReq req);
  Task<IActionResult> HandleGetOrderById(Guid id);
  Task<IActionResult> HandleGetAllOrders(OrderQuery query);
  Task<IActionResult> HandleAdminGetOrders(OrderQuery query);
  Task<OrderDto> CreateOrder(CreateOrderMsg msg);
}

public class PaymentService : BaseService, IPaymentService
{
  private readonly ILogger<PaymentService> _logger;
  private readonly IMapper _mapper;
  private readonly MuseTrip360DbContext _dbContext;
  private readonly IQueuePublisher _queuePub;
  private readonly IOrderRepository _orderRepo;

  public PaymentService(
    ILogger<PaymentService> logger,
    IMapper mapper,
    MuseTrip360DbContext dbContext,
    IQueuePublisher queuePublisher,
    IHttpContextAccessor httpContextAccessor) : base(dbContext, mapper, httpContextAccessor)
  {
    _logger = logger;
    _mapper = mapper;
    _dbContext = dbContext;
    _queuePub = queuePublisher;
    _orderRepo = new OrderRepository(dbContext);
  }

  public async Task<IActionResult> HandleAdminGetOrders(OrderQuery query)
  {
    var orders = await _orderRepo.GetAllAsync();
    return SuccessResp.Ok(_mapper.Map<List<OrderDto>>(orders));
  }

  public async Task<IActionResult> HandleGetAllOrders(OrderQuery query)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var orders = await _orderRepo.GetByUserIdAsync(payload.UserId, query);
    return SuccessResp.Ok(_mapper.Map<List<OrderDto>>(orders));
  }

  public async Task<IActionResult> HandleGetOrderById(Guid id)
  {
    var order = await _orderRepo.GetByIdAsync(id);
    return SuccessResp.Ok(_mapper.Map<OrderDto>(order));
  }

  public async Task<IActionResult> HandleCreateOrder(CreateOrderReq req)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    // TODO: validate req and params

    var msg = _mapper.Map<CreateOrderMsg>(req);
    msg.CreatedBy = payload.UserId;

    await _queuePub.Publish(QueueConst.Order, msg);

    return SuccessResp.Ok("Order created successfully");
  }



  public async Task<OrderDto> CreateOrder(CreateOrderMsg msg)
  {
    var order = _mapper.Map<Order>(msg);
    order.CreatedBy = msg.CreatedBy;
    order.Status = PaymentStatusEnum.Pending;
    order.OrderType = msg.OrderType;
    order.Metadata = msg.Metadata;

    switch (order.OrderType)
    {
      case OrderTypeEnum.Event:
        order.OrderEvents = [.. msg.ItemIds.Select(itemId => new OrderEvent
        {
          EventId = itemId,
        })];
        break;
      case OrderTypeEnum.Tour:
        order.OrderTours = [.. msg.ItemIds.Select(itemId => new OrderTour
        {
          TourId = itemId,
        })];
        break;
      case OrderTypeEnum.Subscription:
        // TODO: handle subscription order
        break;
      default:
        throw new Exception("Invalid order type");
    }

    var result = await _orderRepo.AddAsync(order);

    return _mapper.Map<OrderDto>(order);
  }
}