namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using Domain.Payment;
using Application.Shared.Enum;
using Application.DTOs.Order;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Util;

public interface IOrderRepository
{
  Task<Order?> GetByIdAsync(Guid id);
  Task<List<Order>> GetAllAsync();
  Task<OrderList> GetByUserIdAsync(Guid userId, OrderQuery query);
  Task<OrderList> GetAllAdminAsync(OrderAdminQuery query);
  Task<List<Order>> GetByStatusAsync(PaymentStatusEnum status);
  Task<List<Order>> GetByOrderTypeAsync(OrderTypeEnum orderType);
  Task<Order> AddAsync(Order order);
  Task<Order> UpdateAsync(Guid orderId, Order order);
  Task<Order> GetByOrderCodeAsync(long orderCode);
  Task<bool> VerifyOrderEventExists(Guid userId, List<Guid> eventIds);
  Task<bool> VerifyOrderTourExists(Guid userId, List<Guid> tourIds);
}

public class OrderList
{
  public IEnumerable<Order> Orders { get; set; } = [];
  public int TotalCount { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
  public int TotalPages { get; set; }
}

public class OrderRepository : IOrderRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public OrderRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Order?> GetByIdAsync(Guid id)
  {
    var order = await _dbContext.Orders
    .Include(o => o.CreatedByUser)
    .Include(o => o.OrderEvents)
    .ThenInclude(oe => oe.Event)
    .Include(o => o.OrderTours)
    .ThenInclude(ot => ot.TourOnline)
    .FirstOrDefaultAsync(o => o.Id == id);
    return order;
  }

  public async Task<List<Order>> GetAllAsync()
  {
    var orders = await _dbContext.Orders
      .OrderByDescending(o => o.CreatedAt)
      .Include(o => o.CreatedByUser)
      .OrderByDescending(o => o.CreatedAt)
      .ToListAsync();
    return orders;
  }

  public async Task<OrderList> GetByUserIdAsync(Guid userId, OrderQuery query)
  {
    var orders = _dbContext.Orders
      .Include(o => o.CreatedByUser)
      .Include(o => o.OrderEvents)
      .ThenInclude(oe => oe.Event)
      .Include(o => o.OrderTours)
      .ThenInclude(ot => ot.TourOnline)
      .Where(o => o.CreatedBy == userId);

    if (!string.IsNullOrEmpty(query.Search))
    {
      orders = orders.Where(o => o.Id.ToString().Contains(query.Search));
    }

    if (query.Status != null)
    {
      orders = orders.Where(o => o.Status == query.Status);
    }

    if (query.OrderType != null)
    {
      orders = orders.Where(o => o.OrderType == query.OrderType);
    }

    orders = orders
    .OrderByDescending(o => o.CreatedAt)
    .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

    return new OrderList
    {
      Orders = await orders.ToListAsync(),
      TotalCount = await orders.CountAsync(),
      Page = query.Page,
      PageSize = query.PageSize,
      TotalPages = (int)Math.Ceiling((double)await orders.CountAsync() / query.PageSize)
    };
  }

  public async Task<List<Order>> GetByStatusAsync(PaymentStatusEnum status)
  {
    var orders = await _dbContext.Orders
      .Where(o => o.Status == status)
      .OrderByDescending(o => o.CreatedAt)
      .Include(o => o.CreatedByUser)
      .ToListAsync();
    return orders;
  }

  public async Task<List<Order>> GetByOrderTypeAsync(OrderTypeEnum orderType)
  {
    var orders = await _dbContext.Orders
      .Where(o => o.OrderType == orderType)
      .OrderByDescending(o => o.CreatedAt)
      .Include(o => o.CreatedByUser)
      .ToListAsync();
    return orders;
  }

  public async Task<Order> AddAsync(Order order)
  {
    var result = await _dbContext.Orders.AddAsync(order);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<Order> UpdateAsync(Guid orderId, Order order)
  {
    var existingOrder = await _dbContext.Orders.FindAsync(orderId);
    if (existingOrder == null) return null;

    _dbContext.Entry(existingOrder).CurrentValues.SetValues(order);
    await _dbContext.SaveChangesAsync();
    return existingOrder;
  }

  public async Task<Order> GetByOrderCodeAsync(long orderCode)
  {
    var order = await _dbContext.Orders
      .Include(o => o.CreatedByUser)
      .Include(o => o.OrderEvents)
      .ThenInclude(oe => oe.Event)
      .Include(o => o.OrderTours)
      .ThenInclude(ot => ot.TourOnline)
    .FirstOrDefaultAsync(o => o.OrderCode == orderCode.ToString());
    return order;
  }
  public async Task<bool> VerifyOrderEventExists(Guid userId, List<Guid> eventIds)
  {
    var order = await _dbContext.Orders
      .Include(o => o.OrderEvents)
      .FirstOrDefaultAsync(o => o.CreatedBy == userId && o.OrderEvents.Any(oe => eventIds.Contains(oe.EventId)));
    return order != null;
  }
  public async Task<bool> VerifyOrderTourExists(Guid userId, List<Guid> tourIds)
  {
    var order = await _dbContext.Orders
      .Include(o => o.OrderTours)
      .FirstOrDefaultAsync(o => o.CreatedBy == userId && o.OrderTours.Any(ot => tourIds.Contains(ot.TourId)));
    return order != null;
  }

  public async Task<OrderList> GetAllAdminAsync(OrderAdminQuery query)
  {
    var orders = _dbContext.Orders
      .Include(o => o.CreatedByUser)
      .Include(o => o.OrderEvents)
      .ThenInclude(oe => oe.Event)
      .Include(o => o.OrderTours)
      .ThenInclude(ot => ot.TourOnline)
      .AsQueryable();

    if (!string.IsNullOrEmpty(query.Search))
    {
      orders = orders.Where(o => o.Id.ToString().Contains(query.Search));
    }

    if (query.Status != null)
    {
      orders = orders.Where(o => o.Status == query.Status);
    }

    if (query.OrderType != null)
    {
      orders = orders.Where(o => o.OrderType == query.OrderType);
    }

    orders = orders
    .OrderByDescending(o => o.CreatedAt)
    .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

    return new OrderList
    {
      Orders = await orders.ToListAsync(),
      TotalCount = await orders.CountAsync(),
      Page = query.Page,
      PageSize = query.PageSize,
      TotalPages = (int)Math.Ceiling((double)await orders.CountAsync() / query.PageSize)
    };
  }
}