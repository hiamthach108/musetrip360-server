namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Subscription;
using Application.Shared.Enum;
using Database;
using Domain.Subscription;
using Microsoft.EntityFrameworkCore;

public interface ISubscriptionRepository
{
  Task<Subscription> GetByIdAsync(Guid id);
  Task<Subscription?> GetByIdWithDetailsAsync(Guid id);

  Task<Subscription?> GetByOrderIdAsync(Guid orderId);
  Task<IEnumerable<Subscription>> GetAllAsync(SubscriptionQuery query);
  Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId);
  Task<IEnumerable<Subscription>> GetByUserIdWithDetailsAsync(Guid userId);
  Task<Subscription> AddAsync(Subscription subscription);
  Task<Subscription> UpdateAsync(Guid subscriptionId, Subscription subscription);
  Task<Subscription> DeleteAsync(Subscription subscription);
  Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId);
  Task<Subscription?> GetActiveSubscriptionByUserAndMuseumAsync(Guid userId, Guid museumId);
  Task<Subscription?> GetActiveSubscriptionByMuseumIdAsync(Guid museumId);
  Task<IEnumerable<Subscription>> GetByMuseumIdAsync(Guid museumId);
}

public class SubscriptionRepository : ISubscriptionRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public SubscriptionRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Subscription> GetByIdAsync(Guid id)
  {
    var subscription = await _dbContext.Subscriptions.FindAsync(id);
    return subscription;
  }

  public async Task<Subscription?> GetByIdWithDetailsAsync(Guid id)
  {
    return await _dbContext.Subscriptions
      .Include(s => s.User)
      .Include(s => s.Plan)
      .Include(s => s.Museum)
      .Include(s => s.Order)
      .FirstOrDefaultAsync(s => s.Id == id);
  }

  public async Task<IEnumerable<Subscription>> GetAllAsync()
  {
    var subscriptions = await _dbContext.Subscriptions.ToListAsync();
    return subscriptions;
  }

  public async Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId)
  {
    var subscriptions = await _dbContext.Subscriptions
      .Where(s => s.UserId == userId)
      .OrderByDescending(s => s.CreatedAt)
      .ToListAsync();
    return subscriptions;
  }

  public async Task<IEnumerable<Subscription>> GetByUserIdWithDetailsAsync(Guid userId)
  {
    return await _dbContext.Subscriptions
      .Include(s => s.Plan)
      .Include(s => s.Museum)
      .Include(s => s.Order)
      .Where(s => s.UserId == userId)
      .OrderByDescending(s => s.CreatedAt)
      .ToListAsync();
  }

  public async Task<Subscription?> GetByOrderIdAsync(Guid orderId)
  {
    return await _dbContext.Subscriptions
      .FirstOrDefaultAsync(s => s.OrderId == orderId);
  }

  public async Task<Subscription> AddAsync(Subscription subscription)
  {
    var result = await _dbContext.Subscriptions.AddAsync(subscription);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<Subscription> UpdateAsync(Guid subscriptionId, Subscription subscription)
  {
    var existingSubscription = await _dbContext.Subscriptions.FindAsync(subscriptionId);
    if (existingSubscription == null) return null;

    _dbContext.Entry(existingSubscription).CurrentValues.SetValues(subscription);
    await _dbContext.SaveChangesAsync();
    return existingSubscription;
  }

  public async Task<Subscription> DeleteAsync(Subscription subscription)
  {
    var result = _dbContext.Subscriptions.Remove(subscription);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId)
  {
    return await _dbContext.Subscriptions
      .Where(s => s.UserId == userId && s.Status == SubscriptionStatusEnum.Active)
      .OrderByDescending(s => s.CreatedAt)
      .FirstOrDefaultAsync();
  }

  public async Task<Subscription?> GetActiveSubscriptionByUserAndMuseumAsync(Guid userId, Guid museumId)
  {
    return await _dbContext.Subscriptions
      .Where(s => s.UserId == userId
                && s.MuseumId == museumId
                && s.Status == SubscriptionStatusEnum.Active
                && s.EndDate > DateTime.UtcNow)
      .FirstOrDefaultAsync();
  }

  public async Task<IEnumerable<Subscription>> GetByMuseumIdAsync(Guid museumId)
  {
    return await _dbContext.Subscriptions
      .Include(s => s.User)
      .Include(s => s.Plan)
      .Where(s => s.MuseumId == museumId)
      .OrderByDescending(s => s.CreatedAt)
      .ToListAsync();
  }

  public async Task<IEnumerable<Subscription>> GetAllAsync(SubscriptionQuery query)
  {
    var subscriptions = _dbContext.Subscriptions.AsQueryable();

    if (query.MuseumId.HasValue)
    {
      subscriptions = subscriptions.Where(s => s.MuseumId == query.MuseumId.Value);
    }

    if (query.PlanId.HasValue)
    {
      subscriptions = subscriptions.Where(s => s.PlanId == query.PlanId.Value);
    }

    if (query.Status.HasValue)
    {
      subscriptions = subscriptions.Where(s => s.Status == query.Status.Value);
    }

    return await subscriptions
      .Include(s => s.User)
      .Include(s => s.Plan)
      .Include(s => s.Museum)
      .OrderByDescending(s => s.CreatedAt)
      .Skip((query.Page - 1) * query.PageSize)
      .Take(query.PageSize)
      .ToListAsync();
  }

  public async Task<Subscription?> GetActiveSubscriptionByMuseumIdAsync(Guid museumId)
  {
    return await _dbContext.Subscriptions
      .Where(s => s.MuseumId == museumId && s.Status == SubscriptionStatusEnum.Active)
      .OrderByDescending(s => s.CreatedAt)
      .FirstOrDefaultAsync();
  }
}