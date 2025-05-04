namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Museum;
using Database;
using Domain.Museums;

public interface IMuseumRepository
{
  Museum? GetById(Guid id);
  MuseumList GetAll(MuseumQuery query);
  Task AddAsync(Museum museum);
  Task UpdateAsync(Guid id, Museum museum);
  Task DeleteAsync(Museum museum);
}

public class MuseumList
{
  public List<Museum> Museums { get; set; } = new List<Museum>();
  public int Total { get; set; }
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

  public MuseumList GetAll(MuseumQuery query)
  {
    var queryable = _context.Museums.AsQueryable();
    if (!string.IsNullOrEmpty(query.SearchQuery))
    {
      queryable = queryable.Where(m => m.Name.Contains(query.SearchQuery));
    }

    var total = queryable.Count();
    var museums = queryable.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

    return new MuseumList
    {
      Museums = museums,
      Total = total
    };
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