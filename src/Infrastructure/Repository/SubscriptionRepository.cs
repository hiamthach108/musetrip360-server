namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using Domain.Subscription;

public interface ISubscriptionRepository
{
  Task<Subscription> GetByIdAsync(Guid id);
  Task<IEnumerable<Subscription>> GetAllAsync();
  Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId);
  Task<Subscription> AddAsync(Subscription subscription);
  Task<Subscription> UpdateAsync(Guid subscriptionId, Subscription subscription);
  Task<Subscription> DeleteAsync(Subscription subscription);
  Task<Subscription> GetActiveSubscriptionByUserIdAsync(Guid userId);
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

  public async Task<IEnumerable<Subscription>> GetAllAsync()
  {
    var subscriptions = _dbContext.Subscriptions.AsEnumerable();
    return subscriptions;
  }

  public async Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId)
  {
    var subscriptions = _dbContext.Subscriptions
      .Where(s => s.UserId == userId)
      .OrderByDescending(s => s.CreatedAt)
      .AsEnumerable();
    return subscriptions;
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

  public async Task<Subscription> GetActiveSubscriptionByUserIdAsync(Guid userId)
  {
    var subscription = _dbContext.Subscriptions
      .Where(s => s.UserId == userId && s.Status == Application.Shared.Enum.SubscriptionStatusEnum.Active)
      .OrderByDescending(s => s.CreatedAt)
      .FirstOrDefault();
    return subscription;
  }
}