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
using Domain.Events;
using Domain.Payment;
using Domain.Tours;
using Domain.Subscription;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Net.payOS.Types;

public interface IPaymentService
{
  Task<IActionResult> HandleCreateOrder(CreateOrderReq req);
  Task<IActionResult> HandleGetOrderById(Guid id);
  Task<IActionResult> HandleGetOrdersByUser(OrderQuery query);
  Task<IActionResult> HandleAdminGetOrders(OrderAdminQuery query);
  Task<OrderDto> CreateOrder(CreateOrderMsg msg);
  Task<IActionResult> HandleGetOrderByCode(string orderCode);

  // BankAccount management methods
  Task<IActionResult> HandleGetBankAccountsByMuseum(Guid museumId);
  Task<IActionResult> HandleGetBankAccountById(Guid id);
  Task<IActionResult> HandleCreateBankAccount(Guid museumId, BankAccountCreateDto dto);
  Task<IActionResult> HandleUpdateBankAccount(Guid id, BankAccountUpdateDto dto);
  Task<IActionResult> HandleDeleteBankAccount(Guid id);
  Task<IActionResult> HandlePayosWebhook(WebhookType data);
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
  private readonly IPaymentRepository _paymentRepo;
  private readonly IEventParticipantRepository _eventParticipantRepo;
  private readonly IWalletRepository _walletRepo;
  private readonly IMuseumRepository _museumRepo;
  private readonly ISubscriptionRepository _subscriptionRepo;
  private readonly ITransactionRepository _transactionRepo;
  public PaymentService(
  IConfiguration configuration,
  ILogger<PaymentService> logger,
  IMapper mapper,
  MuseTrip360DbContext dbContext,
  IQueuePublisher queuePublisher,
  IPayOSService payOSService,
  IHttpContextAccessor httpContextAccessor) : base(dbContext, mapper, httpContextAccessor)
  {
    _configuration = configuration;
    _logger = logger;
    _queuePub = queuePublisher;
    _orderRepo = new OrderRepository(dbContext);
    _bankAccountRepo = new BankAccountRepository(dbContext);
    _payOSService = payOSService;
    _eventRepo = new EventRepository(dbContext);
    _tourRepo = new TourOnlineRepository(dbContext);
    _paymentRepo = new PaymentRepository(dbContext);
    _eventParticipantRepo = new EventParticipantRepository(dbContext);
    _walletRepo = new WalletRepository(dbContext);
    _museumRepo = new MuseumRepository(dbContext);
    _subscriptionRepo = new SubscriptionRepository(dbContext);
    _transactionRepo = new TransactionRepository(dbContext);
  }

  public async Task<IActionResult> HandleAdminGetOrders(OrderAdminQuery query)
  {
    try
    {
      var orders = await _orderRepo.GetAllAdminAsync(query);
      return SuccessResp.Ok(new
      {
        List = _mapper.Map<List<OrderDto>>(orders.Orders),
        Total = orders.TotalCount
      });
    }
    catch (Exception e)
    {
      return ErrorResp.InternalServerError(e.Message);
    }
  }

  public async Task<IActionResult> HandleGetOrdersByUser(OrderQuery query)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var orders = await _orderRepo.GetByUserIdAsync(payload.UserId, query);
    return SuccessResp.Ok(new
    {
      List = _mapper.Map<List<OrderDto>>(orders.Orders),
      Total = orders.TotalCount
    });
  }

  public async Task<IActionResult> HandleGetOrderById(Guid id)
  {
    var order = await _orderRepo.GetByIdAsync(id);
    return SuccessResp.Ok(_mapper.Map<OrderDto>(order));
  }

  public async Task<IActionResult> HandleCreateOrder(CreateOrderReq req)
  {
    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
    {
      try
      {
        var payload = ExtractPayload();
        if (payload == null)
        {
          return ErrorResp.Unauthorized("Invalid token");
        }
        var listItem = new List<ItemData>();
        var totalAmount = 0;

        if (req.OrderType == OrderTypeEnum.Event)
        {
          var freeEventExclude = new List<Guid>();
          var orderEventExists = await _orderRepo.VerifyOrderEventExists(payload.UserId, req.ItemIds);
          if (orderEventExists)
          {
            return ErrorResp.BadRequest("Event already exists in order");
          }
          var events = new List<Event>();
          foreach (var itemId in req.ItemIds)
          {
            var ev = await _eventRepo.GetEventById(itemId);
            if (ev == null)
            {
              return ErrorResp.NotFound("Event not found");
            }
            if (ev.AvailableSlots == 0)
            {
              return ErrorResp.BadRequest("Event is full");
            }
            if (ev.Price == 0)
            {
              freeEventExclude.Add(ev.Id);
              // create order
              var order = new Order();
              order.CreatedBy = payload.UserId;
              order.Status = PaymentStatusEnum.Success;
              order.OrderType = req.OrderType;
              order.TotalAmount = 0;
              order.OrderCode = "free-event";
              order.OrderEvents = [new OrderEvent { EventId = ev.Id }];
              await _orderRepo.AddAsync(order);
              // create payment
              var payment = new Payment();
              payment.OrderId = order.Id;
              payment.Amount = 0;
              payment.Status = PaymentStatusEnum.Success;
              payment.PaymentMethod = PaymentMethodEnum.Cash; // free event default is cash 
              payment.CreatedBy = payload.UserId;
              await _paymentRepo.AddAsync(payment);
              // create event participant
              var eventParticipant = new EventParticipant();
              eventParticipant.EventId = ev.Id;
              eventParticipant.UserId = payload.UserId;
              eventParticipant.JoinedAt = DateTime.UtcNow;
              eventParticipant.Role = ParticipantRoleEnum.Attendee;
              eventParticipant.Status = ParticipantStatusEnum.Confirmed;
              await _eventParticipantRepo.AddAsync(eventParticipant);
              // decrease the available slots
              ev.AvailableSlots--;
              await _eventRepo.UpdateAsync(ev.Id, ev);
            }
            else
            {
              events.Add(ev);
            }
          }
          // exclude free event from list item
          if (freeEventExclude.Count > 0)
          {
            req.ItemIds = req.ItemIds.Where(id => !freeEventExclude.Contains(id)).ToList();
          }

          listItem.AddRange(events
            .Where(e => e != null)
            .Select(e => new ItemData(e!.Id.ToString(), 1, (int)e!.Price)));

          totalAmount = (int)events.Sum(e => e!.Price);
        }
        else if (req.OrderType == OrderTypeEnum.Tour)
        {
          var orderTourExists = await _orderRepo.VerifyOrderTourExists(payload.UserId, req.ItemIds);
          if (orderTourExists)
          {
            return ErrorResp.BadRequest("Tour already exists in order");
          }
          var tours = new List<TourOnline>();
          foreach (var itemId in req.ItemIds)
          {
            var tour = await _tourRepo.GetByIdAsync(itemId);
            if (tour == null)
            {
              return ErrorResp.NotFound("Tour not found");
            }
            tours.Add(tour);
          }
          listItem.AddRange(tours.Where(t => t != null).Select(t => new ItemData(t!.Id.ToString(), 1, (int)t.Price)));
          totalAmount = (int)tours.Sum(t => t.Price);
        }
        // if the they only buy free event, return message
        if (totalAmount == 0 && req.ItemIds.Count == 0)
        {
          return SuccessResp.Ok("Order is free");
        }
        var snowflake = new SnowflakeId(1);
        var orderCode = snowflake.GenerateOrderId();
        var paymentData = new PaymentData(
          orderCode: orderCode,
          amount: totalAmount,
          description: $"Payment for {orderCode}",
          items: listItem,
          cancelUrl: req.CancelUrl,
          returnUrl: req.ReturnUrl
        );

        var paymentResult = await _payOSService.CreatePayment(paymentData);

        var msg = _mapper.Map<CreateOrderMsg>(req);
        msg.CreatedBy = payload.UserId;
        msg.OrderCode = paymentResult.orderCode.ToString();
        msg.TotalAmount = totalAmount;
        msg.ExpiredAt = paymentResult.expiredAt.HasValue ? DateTime.UnixEpoch.AddSeconds(paymentResult.expiredAt.Value) : DateTime.UtcNow.AddDays(1);
        msg.Metadata = JsonDocument.Parse(JsonSerializer.Serialize(paymentResult));
        await _queuePub.Publish(QueueConst.Order, msg);
        // commit change
        await transaction.CommitAsync();
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
          req.CancelUrl,
          req.ReturnUrl
        });
      }
      catch (System.Exception)
      {
        await transaction.RollbackAsync();
        throw;
      }

    }
  }

  public async Task<OrderDto> CreateOrder(CreateOrderMsg msg)
  {
    await using var transaction = await _dbContext.Database.BeginTransactionAsync();
    {
      try
      {
        var order = new Order();
        order.CreatedBy = msg.CreatedBy;
        order.Status = PaymentStatusEnum.Pending;
        order.OrderType = msg.OrderType;
        order.Metadata = msg.Metadata;
        order.TotalAmount = msg.TotalAmount;
        order.ExpiredAt = msg.ExpiredAt;
        order.OrderCode = msg.OrderCode;
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
        await transaction.CommitAsync();
        return _mapper.Map<OrderDto>(order);
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        throw new Exception(ex.Message);
      }
    }
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

  public async Task<IActionResult> HandlePayosWebhook(WebhookType data)
  {
    await using var transaction = await _dbContext.Database.BeginTransactionAsync();
    try
    {
      if (data.desc != "success")
      {
        return ErrorResp.BadRequest("Invalid webhook data");
      }
      if (data.data.orderCode == 123)
      {
        return SuccessResp.Ok(new { success = true }); // webhook test
      }
      var webhookData = _payOSService.VerifyWebhookData(data);
      if (webhookData == null)
      {
        return ErrorResp.BadRequest("Invalid webhook data");
      }

      var order = await _orderRepo.GetByOrderCodeAsync(webhookData.orderCode);
      if (order == null)
      {
        return ErrorResp.NotFound("Order not found");
      }
      // create new payment
      var payment = new Payment
      {
        OrderId = order.Id,
        Amount = webhookData.amount,
        Status = PaymentStatusEnum.Success,
        PaymentMethod = PaymentMethodEnum.PayOS,
        CreatedBy = order.CreatedBy,
      };
      payment.Metadata = JsonDocument.Parse(JsonSerializer.Serialize(data));
      // add payment
      await _paymentRepo.AddAsync(payment);
      // update order status
      order.Status = PaymentStatusEnum.Success;
      await _orderRepo.UpdateAsync(order.Id, order);
      // event case
      if (order.OrderType == OrderTypeEnum.Event)
      {
        var eventParticipants = order.OrderEvents.Select(e => new EventParticipant
        {
          EventId = e.EventId,
          UserId = order.CreatedBy,
          JoinedAt = DateTime.UtcNow,
          Role = ParticipantRoleEnum.Attendee,
          Status = ParticipantStatusEnum.Confirmed,
        });
        await _eventParticipantRepo.AddRangeAsync(eventParticipants);
        // add balance to museum wallet
        var eventIds = eventParticipants.Select(e => e.EventId).ToList();
        foreach (var e in eventIds)
        {
          var eventItem = await _eventRepo.GetEventById(e);
          if (eventItem == null)
          {
            throw new Exception("Event not found");
          }
          if (eventItem.AvailableSlots == 0)
          {
            throw new Exception("Event is full");
          }
          // add to museum balance
          var museum = await _museumRepo.GetByIdAsync(eventItem.MuseumId);
          if (museum == null)
          {
            throw new Exception("Museum not found");
          }
          var wallet = await _walletRepo.GetWalletByMuseumId(museum.Id);
          if (wallet == null)
          {
            wallet = await _walletRepo.InitWallet(museum.Id);
          }
          // create transaction item
          var transactionItem = new Domain.Payment.Transaction
          {
            MuseumId = museum.Id,
            ReferenceId = e.ToString(),
            Amount = (decimal)eventItem.Price,
            TransactionType = "Event",
            BalanceBefore = (decimal)wallet.TotalBalance,
            BalanceAfter = (decimal)wallet.TotalBalance + (decimal)eventItem.Price,
          };
          await _transactionRepo.CreateTransaction(transactionItem);
          // add balance after
          await _walletRepo.AddBalance(wallet.Id, eventItem.Price);
          // decrease event available slots
          eventItem.AvailableSlots--;
          await _eventRepo.UpdateAsync(eventItem.Id, eventItem);
        }
      }
      // tou online case
      if (order.OrderType == OrderTypeEnum.Tour)
      {
        var tourIds = order.OrderTours.Select(t => t.TourId).ToList();
        foreach (var t in tourIds)
        {
          var tourItem = await _tourRepo.GetByIdAsync(t);
          if (tourItem == null)
          {
            throw new Exception("Tour not found");
          }
          // add to museum balance
          var museum = await _museumRepo.GetByIdAsync(tourItem.MuseumId);
          if (museum == null)
          {
            throw new Exception("Museum not found");
          }
          var wallet = await _walletRepo.GetWalletByMuseumId(museum.Id);
          if (wallet == null)
          {
            throw new Exception("Wallet not found");
          }
          // create transaction item
          var transactionItem = new Domain.Payment.Transaction
          {
            MuseumId = museum.Id,
            ReferenceId = t.ToString(),
            Amount = (decimal)tourItem.Price,
            TransactionType = "TourOnline",
            BalanceBefore = (decimal)wallet.TotalBalance,
            BalanceAfter = (decimal)wallet.TotalBalance + (decimal)tourItem.Price,
          };
          await _transactionRepo.CreateTransaction(transactionItem);
          // add balance after
          await _walletRepo.AddBalance(wallet.Id, tourItem.Price);
        }
      }
      if (order.OrderType == OrderTypeEnum.Subscription)
      {
        // Activate subscription when payment is successful
        var subscription = await _subscriptionRepo.GetByOrderIdAsync(order.Id);
        if (subscription != null)
        {
          subscription.Status = SubscriptionStatusEnum.Active;
          await _subscriptionRepo.UpdateAsync(subscription.Id, subscription);
        }
      }
      await transaction.CommitAsync();
      return SuccessResp.Ok(new { success = true });
    }
    catch (Exception e)
    {
      await transaction.RollbackAsync();
      return ErrorResp.InternalServerError(e.Message);
    }
  }

  public async Task<IActionResult> HandleGetOrderByCode(string orderCode)
  {
    try
    {
      var order = await _orderRepo.GetByOrderCodeAsync(long.Parse(orderCode));
      if (order == null)
      {
        return ErrorResp.NotFound("Order not found");
      }
      return SuccessResp.Ok(_mapper.Map<OrderDto>(order));
    }
    catch (Exception e)
    {
      return ErrorResp.InternalServerError(e.Message);
    }
  }
}