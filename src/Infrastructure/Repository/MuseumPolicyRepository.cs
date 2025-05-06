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
  Task<MuseumPolicy> AddAsync(MuseumPolicy policy);
  Task<MuseumPolicy> UpdateAsync(Guid id, MuseumPolicy policy);
  Task DeleteAsync(MuseumPolicy policy);
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
        .Include(p => p.Museum)
        .FirstOrDefault(p => p.Id == id);
  }

  public MuseumPolicyList GetAll(PaginationReq query, Guid museumId)
  {
    var policies = _dbContext.MuseumPolicies
        .Include(p => p.CreatedByUser)
        .Include(p => p.Museum)
        .Where(p => p.MuseumId == museumId)
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
}