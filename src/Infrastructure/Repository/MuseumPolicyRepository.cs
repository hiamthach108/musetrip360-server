namespace Infrastructure.Repository;

using Domain.Museums;
using Application.Shared.Type;
using Database;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.Pagination;

public interface IMuseumPolicyRepository
{
  MuseumPolicy? GetById(Guid id);
  MuseumPolicyList GetAll(PaginationReq query, Guid museumId);
  List<MuseumPolicy> GetAllByMuseumId(Guid museumId);
  Task<MuseumPolicy> AddAsync(MuseumPolicy policy);
  Task<MuseumPolicy> UpdateAsync(Guid id, MuseumPolicy policy);
  Task DeleteAsync(MuseumPolicy policy);
  Task<List<MuseumPolicy>> BulkCreateUpdateAsync(List<MuseumPolicy> policies, Guid museumId);
  Task DeleteByIdsAsync(List<Guid> ids);
}

public class MuseumPolicyList
{
  public List<MuseumPolicy> Policies { get; set; } = new List<MuseumPolicy>();
  public int Total { get; set; }
}

public class MuseumPolicyRepository : IMuseumPolicyRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public MuseumPolicyRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public MuseumPolicy? GetById(Guid id)
  {
    return _dbContext.MuseumPolicies
        .Include(p => p.CreatedByUser)
        .FirstOrDefault(p => p.Id == id);
  }

  public MuseumPolicyList GetAll(PaginationReq query, Guid museumId)
  {
    var policies = _dbContext.MuseumPolicies
        .Include(p => p.CreatedByUser)
        .Where(p => p.MuseumId == museumId)
        .OrderBy(p => p.ZOrder)
        .ThenBy(p => p.CreatedAt)
        .AsQueryable();

    var total = policies.Count();

    policies = policies
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize);

    return new MuseumPolicyList
    {
      Policies = policies.ToList(),
      Total = total
    };
  }

  public List<MuseumPolicy> GetAllByMuseumId(Guid museumId)
  {
    return _dbContext.MuseumPolicies
        .Include(p => p.CreatedByUser)
        .Where(p => p.MuseumId == museumId)
        .OrderBy(p => p.ZOrder)
        .ThenBy(p => p.CreatedAt)
        .ToList();
  }

  public async Task<MuseumPolicy> AddAsync(MuseumPolicy policy)
  {
    await _dbContext.MuseumPolicies.AddAsync(policy);
    await _dbContext.SaveChangesAsync();
    return policy;
  }

  public async Task<MuseumPolicy> UpdateAsync(Guid id, MuseumPolicy policy)
  {
    _dbContext.MuseumPolicies.Update(policy);
    await _dbContext.SaveChangesAsync();
    return policy;
  }

  public async Task DeleteAsync(MuseumPolicy policy)
  {
    _dbContext.MuseumPolicies.Remove(policy);
    await _dbContext.SaveChangesAsync();
  }

  public async Task<List<MuseumPolicy>> BulkCreateUpdateAsync(List<MuseumPolicy> policies, Guid museumId)
  {
    var existingPolicies = await _dbContext.MuseumPolicies
        .Where(p => p.MuseumId == museumId)
        .ToListAsync();

    var result = new List<MuseumPolicy>();

    foreach (var policy in policies)
    {
      var existingPolicy = existingPolicies.FirstOrDefault(p => p.Id == policy.Id);

      if (existingPolicy != null)
      {
        existingPolicy.Title = policy.Title;
        existingPolicy.Content = policy.Content;
        existingPolicy.PolicyType = policy.PolicyType;
        existingPolicy.IsActive = policy.IsActive;
        existingPolicy.ZOrder = policy.ZOrder;

        _dbContext.MuseumPolicies.Update(existingPolicy);
        result.Add(existingPolicy);
      }
      else
      {
        await _dbContext.MuseumPolicies.AddAsync(policy);
        result.Add(policy);
      }
    }

    await _dbContext.SaveChangesAsync();
    return result;
  }

  public async Task DeleteByIdsAsync(List<Guid> ids)
  {
    var policiesToDelete = await _dbContext.MuseumPolicies
        .Where(p => ids.Contains(p.Id))
        .ToListAsync();

    _dbContext.MuseumPolicies.RemoveRange(policiesToDelete);
    await _dbContext.SaveChangesAsync();
  }
}