namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Museum;
using Application.Shared.Enum;
using Database;
using Domain.Museums;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;

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
  Task FeedbackMuseums(Guid museumId, int rating, Guid userId, string comment);
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
      queryable = queryable.Where(m => m.Name.Contains(query.Search) || m.Description.Contains(query.Search));
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

  public async Task FeedbackMuseums(Guid museumId, int rating, Guid userId, string comment)
  {
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      var museum = await _context.Museums.FindAsync(museumId);
      if (museum == null) throw new Exception("Museum not found");

      // find feedback of user
      var userFeedback = await _context.Feedbacks
        .FirstOrDefaultAsync(f => f.TargetId == museumId && f.CreatedBy == userId);

      if (userFeedback != null)
      {
        // update feedback
        userFeedback.Rating = rating;
        userFeedback.Comment = comment;
      }
      else
      {
        // create new feedback
        var newFeedback = new Feedback
        {
          TargetId = museumId,
          Type = DataEntityType.Museum,
          Rating = rating,
          Comment = comment,
          CreatedBy = userId
        };
        await _context.Feedbacks.AddAsync(newFeedback);
      }

      // save changes
      await _context.SaveChangesAsync();

      // calculate average rating
      var listFeedback = await _context.Feedbacks
        .Where(f => f.TargetId == museumId)
        .ToListAsync();
      var averageRating = listFeedback.Average(f => f.Rating);

      // update museum rating
      museum.Rating = (float)Math.Round(averageRating, 1);
      await _context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();
      throw new InvalidOperationException("An error occurred while providing feedback for the tour online.", ex);
    }
    await transaction.CommitAsync();
  }
}