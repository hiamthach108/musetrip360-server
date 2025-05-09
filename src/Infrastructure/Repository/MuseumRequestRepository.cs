namespace Infrastructure.Repository;

using Domain.Museums;
using Application.Shared.Type;
using Database;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.MuseumRequest;

public interface IMuseumRequestRepository
{
  MuseumRequest? GetById(Guid id);
  MuseumRequestList GetAll(MuseumRequestQuery query);
  MuseumRequestList GetByUserId(Guid userId, MuseumRequestQuery query);
  Task<MuseumRequest> AddAsync(MuseumRequest request);
  Task<MuseumRequest> UpdateAsync(Guid id, MuseumRequest request);
  Task DeleteAsync(MuseumRequest request);
}

public class MuseumRequestList
{
  public List<MuseumRequest> Requests { get; set; } = new List<MuseumRequest>();
  public int Total { get; set; }
}

public class MuseumRequestRepository : IMuseumRequestRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public MuseumRequestRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public MuseumRequest? GetById(Guid id)
  {
    return _dbContext.MuseumRequests
        .Include(r => r.CreatedByUser)
        .FirstOrDefault(r => r.Id == id);
  }

  public MuseumRequestList GetAll(MuseumRequestQuery query)
  {
    var requests = _dbContext.MuseumRequests
        .Include(r => r.CreatedByUser)
        .AsQueryable();

    if (!string.IsNullOrEmpty(query.Search))
    {
      requests = requests.Where(r =>
          r.MuseumName.Contains(query.Search) ||
          r.MuseumDescription.Contains(query.Search));
    }

    var total = requests.Count();

    requests = requests
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize);

    return new MuseumRequestList
    {
      Requests = requests.ToList(),
      Total = total
    };
  }

  public MuseumRequestList GetByUserId(Guid userId, MuseumRequestQuery query)
  {
    var requests = _dbContext.MuseumRequests
        .Include(r => r.CreatedByUser)
        .Where(r => r.CreatedBy == userId)
        .AsQueryable();

    if (!string.IsNullOrEmpty(query.Search))
    {
      requests = requests.Where(r =>
          r.MuseumName.Contains(query.Search) ||
          r.MuseumDescription.Contains(query.Search));
    }

    var total = requests.Count();

    requests = requests
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize);

    return new MuseumRequestList
    {
      Requests = requests.ToList(),
      Total = total
    };
  }

  public async Task<MuseumRequest> AddAsync(MuseumRequest request)
  {
    await _dbContext.MuseumRequests.AddAsync(request);
    await _dbContext.SaveChangesAsync();
    return request;
  }

  public async Task<MuseumRequest> UpdateAsync(Guid id, MuseumRequest request)
  {
    _dbContext.MuseumRequests.Update(request);
    await _dbContext.SaveChangesAsync();
    return request;
  }

  public async Task DeleteAsync(MuseumRequest request)
  {
    _dbContext.MuseumRequests.Remove(request);
    await _dbContext.SaveChangesAsync();
  }
}