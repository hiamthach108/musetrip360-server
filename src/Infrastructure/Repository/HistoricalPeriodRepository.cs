namespace Infrastructure.Repository;

using Database;
using Domain.Content;
using Microsoft.EntityFrameworkCore;

public interface IHistoricalPeriodRepository
{
  Task<HistoricalPeriod?> GetByIdAsync(Guid id);
  Task<IEnumerable<HistoricalPeriod>> GetAllAsync();
  Task<HistoricalPeriod> AddAsync(HistoricalPeriod historicalPeriod);
  Task<HistoricalPeriod?> UpdateAsync(Guid id, HistoricalPeriod historicalPeriod);
  Task<bool> DeleteAsync(Guid id);
  Task<bool> ExistsAsync(Guid id);
  Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
  Task<IEnumerable<HistoricalPeriod>> SearchByNameAsync(string? name);
}

public class HistoricalPeriodRepository : IHistoricalPeriodRepository
{
  private readonly MuseTrip360DbContext _context;

  public HistoricalPeriodRepository(MuseTrip360DbContext context)
  {
    _context = context;
  }

  public async Task<HistoricalPeriod?> GetByIdAsync(Guid id)
  {
    return await _context.HistoricalPeriods.FirstOrDefaultAsync(h => h.Id == id);
  }

  public async Task<IEnumerable<HistoricalPeriod>> GetAllAsync()
  {
    return await _context.HistoricalPeriods
      .OrderBy(h => h.EndDate)
      .ThenBy(h => h.Name)
      .ToListAsync();
  }

  public async Task<HistoricalPeriod> AddAsync(HistoricalPeriod historicalPeriod)
  {
    var result = await _context.HistoricalPeriods.AddAsync(historicalPeriod);
    await _context.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<HistoricalPeriod?> UpdateAsync(Guid id, HistoricalPeriod historicalPeriod)
  {
    var existingHistoricalPeriod = await _context.HistoricalPeriods.FindAsync(id);
    if (existingHistoricalPeriod == null)
      return null;

    existingHistoricalPeriod.Name = historicalPeriod.Name;
    existingHistoricalPeriod.Description = historicalPeriod.Description;
    existingHistoricalPeriod.StartDate = historicalPeriod.StartDate;
    existingHistoricalPeriod.EndDate = historicalPeriod.EndDate;
    existingHistoricalPeriod.Metadata = historicalPeriod.Metadata;

    await _context.SaveChangesAsync();
    return existingHistoricalPeriod;
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    var historicalPeriod = await _context.HistoricalPeriods.FindAsync(id);
    if (historicalPeriod == null)
      return false;

    _context.HistoricalPeriods.Remove(historicalPeriod);
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<bool> ExistsAsync(Guid id)
  {
    return await _context.HistoricalPeriods.AnyAsync(h => h.Id == id);
  }

  public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
  {
    var query = _context.HistoricalPeriods.Where(h => h.Name == name);
    if (excludeId.HasValue)
    {
      query = query.Where(h => h.Id != excludeId.Value);
    }
    return await query.AnyAsync();
  }

  public async Task<IEnumerable<HistoricalPeriod>> SearchByNameAsync(string? name)
  {
    var query = _context.HistoricalPeriods.AsQueryable();

    if (!string.IsNullOrWhiteSpace(name))
    {
      query = query.Where(h => h.Name.Contains(name) ||
                              (h.Description != null && h.Description.Contains(name)));
    }

    return await query
      .OrderBy(h => h.Name)
      .ToListAsync();
  }
}