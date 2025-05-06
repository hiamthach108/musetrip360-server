namespace Infrastructure.Repository;

using Domain.Museums;
using Application.Shared.Type;
using Database;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.MuseumRequest;

public interface IMuseumRequestRepository
{
  Task<MuseumRequest> GetByIdAsync(Guid id);
  Task<(IEnumerable<MuseumRequest> Requests, int Total)> GetAllAsync(MuseumRequestQuery query);
  Task<MuseumRequest> AddAsync(MuseumRequest request);
  Task<MuseumRequest> UpdateAsync(Guid id, MuseumRequest request);
  Task DeleteAsync(MuseumRequest request);
}

public class MuseumRequestRepository : IMuseumRequestRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public MuseumRequestRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<MuseumRequest> GetByIdAsync(Guid id)
  {
    return await _dbContext.MuseumRequests
        .Include(r => r.CreatedByUser)
        .FirstOrDefaultAsync(r => r.Id == id);
  }

  public async Task<(IEnumerable<MuseumRequest> Requests, int Total)> GetAllAsync(MuseumRequestQuery query)
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

    var total = await requests.CountAsync();

    requests = requests
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize);

    return (await requests.ToListAsync(), total);
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