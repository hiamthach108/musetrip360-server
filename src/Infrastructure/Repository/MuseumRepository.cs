namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Museum;
using Application.Shared.Enum;
using Database;
using Domain.Museums;

public interface IMuseumRepository
{
  Museum? GetById(Guid id);
  IEnumerable<Museum> GetByIds(IEnumerable<Guid> ids);
  bool IsMuseumExists(Guid id);
  bool IsMuseumNameExists(string name);
  Museum? GetByName(string name);
  MuseumList GetAll(MuseumQuery query);
  MuseumList GetAllAdmin(MuseumQuery query);
  Task<Museum> AddAsync(Museum museum);
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

  public IEnumerable<Museum> GetByIds(IEnumerable<Guid> ids)
  {
    return _context.Museums.Where(m => ids.Contains(m.Id));
  }

  public bool IsMuseumExists(Guid id)
  {
    return _context.Museums.Any(m => m.Id == id);
  }

  public bool IsMuseumNameExists(string name)
  {
    return _context.Museums.Any(m => m.Name == name);
  }

  public Museum? GetByName(string name)
  {
    return _context.Museums.FirstOrDefault(m => m.Name == name);
  }

  public MuseumList GetAll(MuseumQuery query)
  {
    var queryable = _context.Museums.AsQueryable();
    queryable = queryable.Where(m => m.Status == MuseumStatusEnum.Active);
    queryable = queryable.OrderByDescending(m => m.UpdatedAt);
    if (!string.IsNullOrEmpty(query.Search))
    {
      queryable = queryable.Where(m => m.Name.Contains(query.Search));
      queryable = queryable.Where(m => m.Description.Contains(query.Search));
    }

    var total = queryable.Count();
    var museums = queryable.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

    return new MuseumList
    {
      Museums = museums,
      Total = total
    };
  }

  public MuseumList GetAllAdmin(MuseumQuery query)
  {
    var queryable = _context.Museums.AsQueryable();
    queryable = queryable.OrderByDescending(m => m.UpdatedAt);
    if (!string.IsNullOrEmpty(query.Search))
    {
      queryable = queryable.Where(m => m.Name.Contains(query.Search));
    }

    if (query.Status != null)
    {
      queryable = queryable.Where(m => m.Status == query.Status);
    }

    var total = queryable.Count();
    var museums = queryable.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

    return new MuseumList
    {
      Museums = museums,
      Total = total
    };
  }

  public async Task<Museum> AddAsync(Museum museum)
  {
    var result = await _context.Museums.AddAsync(museum);
    await _context.SaveChangesAsync();
    return result.Entity;
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