namespace Application.Service;

using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Application.DTOs.Order;
using Application.DTOs.Payment;
using Application.Shared.Constant;
using Application.Shared.Enum;
using Application.Shared.Helpers;
using Application.Shared.Type;
using AutoMapper;
using Core.Payos;
using Core.Queue;
using Database;
using Domain.Payment;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;

public interface IPaymentService
{
  Task<IActionResult> HandleCreateOrder(CreateOrderReq req);
  Task<IActionResult> HandleGetOrderById(Guid id);
  Task<IActionResult> HandleGetAllOrders(OrderQuery query);
  Task<IActionResult> HandleAdminGetOrders(OrderQuery query);
  Task<OrderDto> CreateOrder(CreateOrderMsg msg);

  // BankAccount management methods
  Task<IActionResult> HandleGetBankAccountsByMuseum(Guid museumId);
  Task<IActionResult> HandleGetBankAccountById(Guid id);
  Task<IActionResult> HandleCreateBankAccount(Guid museumId, BankAccountCreateDto dto);
  Task<IActionResult> HandleUpdateBankAccount(Guid id, BankAccountUpdateDto dto);
  Task<IActionResult> HandleDeleteBankAccount(Guid id);
}

public class PaymentService : BaseService, IPaymentService
{
  private readonly IPayOSService _payOSService;
  private readonly IConfiguration _configuration;
  private readonly ILogger<PaymentService> _logger;
  private readonly IQueuePublisher _queuePub;
  private readonly IOrderRepository _orderRepo;
  private readonly IBankAccountRepository _bankAccountRepo;
  private readonly IEventRepository _eventRepo;
  private readonly ITourOnlineRepository _tourRepo;

  public PaymentService(
    IConfiguration configuration,
    ILogger<PaymentService> logger,
    IMapper mapper,
    MuseTrip360DbContext dbContext,
    IQueuePublisher queuePublisher,
    IPayOSService payOSService,
    IEventRepository eventRepo,
    ITourOnlineRepository tourRepo,
    IHttpContextAccessor httpContextAccessor) : base(dbContext, mapper, httpContextAccessor)
  {
    _configuration = configuration;
    _logger = logger;
    _queuePub = queuePublisher;
    _orderRepo = new OrderRepository(dbContext);
    _bankAccountRepo = new BankAccountRepository(dbContext);
    _payOSService = payOSService;
    _eventRepo = eventRepo;
    _tourRepo = tourRepo;
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
    var listItem = new List<ItemData>();
    if (req.OrderType == OrderTypeEnum.Event)
    {
      var listEvent = req.ItemIds.Select(async itemId => await _eventRepo.GetEventById(itemId)).ToList();
      // price set to 0
      listItem.AddRange(listEvent.Select(e => new ItemData(e.Id.ToString(), 1, 0)).ToList());
    }
    else if (req.OrderType == OrderTypeEnum.Tour)
    {
      var listTour = req.ItemIds.Select(async itemId => await _tourRepo.GetByIdAsync(itemId)).ToList();
      listItem.AddRange(listTour.Select(t => new ItemData(t.Id.ToString(), 1, 0)).ToList());
    }

    var snowflake = new SnowflakeId(1);
    var paymentData = new PaymentData(
      orderCode: snowflake.GenerateOrderId(),
      amount: (int)req.TotalAmount,
      description: "Payment for order",
      items: listItem,
      cancelUrl: $"{_configuration["Frontend:Url"]}/payment/cancel",
      returnUrl: $"{_configuration["Frontend:Url"]}/payment/success"
    );

    var paymentResult = await _payOSService.CreatePayment(paymentData);

    var msg = _mapper.Map<CreateOrderMsg>(req);
    msg.CreatedBy = payload.UserId;

    await _queuePub.Publish(QueueConst.Order, msg);

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
    });
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

  // BankAccount management methods
  public async Task<IActionResult> HandleGetBankAccountsByMuseum(Guid museumId)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var result = await _bankAccountRepo.GetByMuseumIdAsync(museumId);
    var bankAccounts = _mapper.Map<IEnumerable<BankAccountDto>>(result);

    var response = new
    {
      Data = bankAccounts,
    };

    return SuccessResp.Ok(response);
  }

  public async Task<IActionResult> HandleGetBankAccountById(Guid id)
  {
    var bankAccount = await _bankAccountRepo.GetByIdAsync(id);
    if (bankAccount == null)
    {
      return ErrorResp.NotFound("Bank account not found");
    }

    var result = _mapper.Map<BankAccountDto>(bankAccount);
    return SuccessResp.Ok(result);
  }

  public async Task<IActionResult> HandleCreateBankAccount(Guid museumId, BankAccountCreateDto dto)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    // Check if account number already exists for this museum
    var exists = await _bankAccountRepo.ExistsAccountNumberForMuseumAsync(dto.AccountNumber, museumId);
    if (exists)
    {
      return ErrorResp.BadRequest("Account number already exists for this museum");
    }

    var bankAccount = _mapper.Map<BankAccount>(dto);
    bankAccount.MuseumId = museumId;

    var result = await _bankAccountRepo.AddAsync(bankAccount);
    var responseDto = _mapper.Map<BankAccountDto>(result);

    return SuccessResp.Created(responseDto);
  }

  public async Task<IActionResult> HandleUpdateBankAccount(Guid id, BankAccountUpdateDto dto)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var existingBankAccount = await _bankAccountRepo.GetByIdAsync(id);
    if (existingBankAccount == null)
    {
      return ErrorResp.NotFound("Bank account not found");
    }

    // Check if account number already exists for this museum (excluding current one)
    if (!string.IsNullOrEmpty(dto.AccountNumber))
    {
      var exists = await _bankAccountRepo.ExistsAccountNumberForMuseumAsync(dto.AccountNumber, existingBankAccount.MuseumId, id);
      if (exists)
      {
        return ErrorResp.BadRequest("Account number already exists for this museum");
      }
    }

    _mapper.Map(dto, existingBankAccount);
    var result = await _bankAccountRepo.UpdateAsync(id, existingBankAccount);

    if (result == null)
    {
      return ErrorResp.NotFound("Bank account not found");
    }

    var responseDto = _mapper.Map<BankAccountDto>(result);
    return SuccessResp.Ok(responseDto);
  }

  public async Task<IActionResult> HandleDeleteBankAccount(Guid id)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var result = await _bankAccountRepo.DeleteAsync(id);
    if (result == null)
    {
      return ErrorResp.NotFound("Bank account not found");
    }

    return SuccessResp.Ok("Bank account deleted successfully");
  }
}