namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using Domain.Payment;
using Application.Shared.Enum;
using Application.DTOs.Order;

public interface IOrderRepository
{
  Task<Order> GetByIdAsync(Guid id);
  Task<IEnumerable<Order>> GetAllAsync();
  Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, OrderQuery query);
  Task<IEnumerable<Order>> GetByStatusAsync(PaymentStatusEnum status);
  Task<IEnumerable<Order>> GetByOrderTypeAsync(OrderTypeEnum orderType);
  Task<Order> AddAsync(Order order);
  Task<Order> UpdateAsync(Guid orderId, Order order);
}

public class OrderRepository : IOrderRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public OrderRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Order> GetByIdAsync(Guid id)
  {
    var order = await _dbContext.Orders.FindAsync(id);
    return order;
  }

  public async Task<IEnumerable<Order>> GetAllAsync()
  {
    var orders = _dbContext.Orders
      .OrderByDescending(o => o.CreatedAt)
      .AsEnumerable();
    return orders;
  }

  public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, OrderQuery query)
  {
    var orders = _dbContext.Orders.Where(o => o.CreatedBy == userId);

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

    orders = orders.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

    return orders;
  }

  public async Task<IEnumerable<Order>> GetByStatusAsync(PaymentStatusEnum status)
  {
    var orders = _dbContext.Orders
      .Where(o => o.Status == status)
      .OrderByDescending(o => o.CreatedAt)
      .AsEnumerable();
    return orders;
  }

  public async Task<IEnumerable<Order>> GetByOrderTypeAsync(OrderTypeEnum orderType)
  {
    var orders = _dbContext.Orders
      .Where(o => o.OrderType == orderType)
      .OrderByDescending(o => o.CreatedAt)
      .AsEnumerable();
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
}