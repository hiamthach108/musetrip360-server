namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using Domain.Tours;
using Microsoft.EntityFrameworkCore;

public interface ITourViewerRepository
{
  Task<TourViewer?> GetByIdAsync(Guid id);
  Task<IEnumerable<TourViewer>> GetAllAsync();
  Task<IEnumerable<TourViewer>> GetByTourIdAsync(Guid tourId);
  Task<IEnumerable<TourViewer>> GetByUserIdAsync(Guid userId);
  Task<TourViewer?> GetByTourIdAndUserIdAsync(Guid tourId, Guid userId);
  Task<IEnumerable<TourViewer>> GetActiveTourViewersAsync(Guid tourId);
  Task<TourViewer> AddAsync(TourViewer tourViewer);
  Task<TourViewer> UpdateAsync(Guid tourViewerId, TourViewer tourViewer);
  Task<TourViewer> DeleteAsync(TourViewer tourViewer);
}

public class TourViewerRepository : ITourViewerRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public TourViewerRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<TourViewer?> GetByIdAsync(Guid id)
  {
    var tourViewer = await _dbContext.TourViewers.FindAsync(id);
    return tourViewer;
  }

  public async Task<IEnumerable<TourViewer>> GetAllAsync()
  {
    var tourViewers = await _dbContext.TourViewers.ToListAsync();
    return tourViewers;
  }

  public async Task<IEnumerable<TourViewer>> GetByTourIdAsync(Guid tourId)
  {
    var tourViewers = await _dbContext.TourViewers
      .Where(tv => tv.TourId == tourId)
      .OrderByDescending(tv => tv.LastViewedAt)
      .ToListAsync();
    return tourViewers;
  }

  public async Task<IEnumerable<TourViewer>> GetByUserIdAsync(Guid userId)
  {
    var tourViewers = await _dbContext.TourViewers
      .Where(tv => tv.UserId == userId)
      .OrderByDescending(tv => tv.LastViewedAt)
      .ToListAsync();
    return tourViewers;
  }

  public async Task<TourViewer?> GetByTourIdAndUserIdAsync(Guid tourId, Guid userId)
  {
    var tourViewer = await _dbContext.TourViewers
      .FirstOrDefaultAsync(tv => tv.TourId == tourId && tv.UserId == userId);
    return tourViewer;
  }

  public async Task<IEnumerable<TourViewer>> GetActiveTourViewersAsync(Guid tourId)
  {
    var tourViewers = await _dbContext.TourViewers
      .Where(tv => tv.TourId == tourId && tv.IsActive)
      .OrderByDescending(tv => tv.LastViewedAt)
      .ToListAsync();
    return tourViewers;
  }

  public async Task<TourViewer> AddAsync(TourViewer tourViewer)
  {
    var result = await _dbContext.TourViewers.AddAsync(tourViewer);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<TourViewer> UpdateAsync(Guid tourViewerId, TourViewer tourViewer)
  {
    var existingTourViewer = await _dbContext.TourViewers.FindAsync(tourViewerId);
    if (existingTourViewer == null) return null;

    _dbContext.Entry(existingTourViewer).CurrentValues.SetValues(tourViewer);
    await _dbContext.SaveChangesAsync();
    return existingTourViewer;
  }

  public async Task<TourViewer> DeleteAsync(TourViewer tourViewer)
  {
    var result = _dbContext.TourViewers.Remove(tourViewer);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }
}