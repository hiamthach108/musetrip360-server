
namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using Domain.Museums;

public interface IMuseumRepository
{
  Museum? GetById(Guid id);
  IEnumerable<Museum> GetAllAsync();
  Task AddAsync(Museum museum);
  Task UpdateAsync(Guid id, Museum museum);
  Task DeleteAsync(Museum museum);
}

public class MuseumRepository : IMuseumRepository
{
  private readonly MuseTrip360DbContext _context;

  public MuseumRepository(MuseTrip360DbContext context)
  {
    _context = context;
  }

  public Museum? GetById(Guid id)
  {
    return _context.Museums.Find(id);
  }

  public IEnumerable<Museum> GetAllAsync()
  {
    return _context.Museums.ToList();
  }

  public async Task AddAsync(Museum museum)
  {
    await _context.Museums.AddAsync(museum);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateAsync(Guid id, Museum museum)
  {
    var existingMuseum = GetById(id);
    if (existingMuseum != null)
    {
      _context.Entry(existingMuseum).CurrentValues.SetValues(museum);
      await _context.SaveChangesAsync();
    }
  }

  public async Task DeleteAsync(Museum museum)
  {
    _context.Museums.Remove(museum);
    await _context.SaveChangesAsync();
  }
}