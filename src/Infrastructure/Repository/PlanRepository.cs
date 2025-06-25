namespace Infrastructure.Repository;

using Domain.Subscription;
using Database;
using Microsoft.EntityFrameworkCore;

public interface IPlanRepository
{
  Task<Plan?> GetByIdAsync(Guid id);
  Task<IEnumerable<Plan>> GetAllAsync();
  Task<IEnumerable<Plan>> GetActiveAsync();
  Task<Plan> AddAsync(Plan plan);
  Task<Plan> UpdateAsync(Plan plan);
  Task<Plan> DeleteAsync(Plan plan);
  Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
  Task<IEnumerable<Plan>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
  Task<IEnumerable<Plan>> GetByDurationAsync(int minDays, int maxDays);
  Task<IEnumerable<Plan>> GetPopularAsync(int limit = 5);
}

public class PlanRepository : IPlanRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public PlanRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Plan?> GetByIdAsync(Guid id)
  {
    return await _dbContext.Plans
        .Include(p => p.Subscriptions)
        .FirstOrDefaultAsync(p => p.Id == id);
  }

  public async Task<IEnumerable<Plan>> GetAllAsync()
  {
    return await _dbContext.Plans
        .Include(p => p.Subscriptions)
        .OrderBy(p => p.Name)
        .ToListAsync();
  }

  public async Task<IEnumerable<Plan>> GetActiveAsync()
  {
    return await _dbContext.Plans
        .Include(p => p.Subscriptions)
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .ToListAsync();
  }

  public async Task<Plan> AddAsync(Plan plan)
  {
    plan.Id = Guid.NewGuid();
    plan.CreatedAt = DateTime.UtcNow;
    plan.UpdatedAt = DateTime.UtcNow;

    _dbContext.Plans.Add(plan);
    await _dbContext.SaveChangesAsync();
    return plan;
  }

  public async Task<Plan> UpdateAsync(Plan plan)
  {
    plan.UpdatedAt = DateTime.UtcNow;

    _dbContext.Plans.Update(plan);
    await _dbContext.SaveChangesAsync();
    return plan;
  }

  public async Task<Plan> DeleteAsync(Plan plan)
  {
    plan.IsActive = false;
    plan.UpdatedAt = DateTime.UtcNow;

    _dbContext.Plans.Update(plan);
    await _dbContext.SaveChangesAsync();
    return plan;
  }

  public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
  {
    var query = _dbContext.Plans.Where(p => p.Name.ToLower() == name.ToLower());

    if (excludeId.HasValue)
    {
      query = query.Where(p => p.Id != excludeId.Value);
    }

    return !await query.AnyAsync();
  }

  public async Task<IEnumerable<Plan>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
  {
    return await _dbContext.Plans
        .Include(p => p.Subscriptions)
        .Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice)
        .OrderBy(p => p.Price)
        .ToListAsync();
  }

  public async Task<IEnumerable<Plan>> GetByDurationAsync(int minDays, int maxDays)
  {
    return await _dbContext.Plans
        .Include(p => p.Subscriptions)
        .Where(p => p.IsActive && p.DurationDays >= minDays && p.DurationDays <= maxDays)
        .OrderBy(p => p.DurationDays)
        .ToListAsync();
  }

  public async Task<IEnumerable<Plan>> GetPopularAsync(int limit = 5)
  {
    return await _dbContext.Plans
        .Include(p => p.Subscriptions)
        .Where(p => p.IsActive)
        .OrderByDescending(p => p.Subscriptions.Count(s => s.Status == Application.Shared.Enum.SubscriptionStatusEnum.Active))
        .Take(limit)
        .ToListAsync();
  }
}