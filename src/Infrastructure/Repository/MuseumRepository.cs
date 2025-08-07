namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Museum;
using Application.Shared.Enum;
using Database;
using Domain.Content;
using Domain.Museums;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;

public interface IMuseumRepository
{
  Museum? GetById(Guid id);
  Task<Museum> GetByIdAsync(Guid id);
  IEnumerable<Museum> GetByIds(IEnumerable<Guid> ids);
  bool IsMuseumExists(Guid id);
  bool IsMuseumNameExists(string name);
  Museum? GetByName(string name);
  MuseumList GetAll(MuseumQuery query);
  MuseumList GetAllAdmin(MuseumQuery query);
  Task<Museum> AddAsync(Museum museum);
  Task<Museum> UpdateAsync(Guid id, Museum museum);
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
  private readonly MuseTrip360DbContext _dbContext;

  public MuseumRepository(MuseTrip360DbContext context)
  {
    _dbContext = context;
  }

  public Museum? GetById(Guid id)
  {
    var museum = _dbContext.Museums.Include(m => m.Categories).FirstOrDefault(m => m.Id == id);

    return museum;
  }

  public async Task<Museum> GetByIdAsync(Guid id)
  {
    var museum = await _dbContext.Museums
      .Include(m => m.Categories)
      .FirstOrDefaultAsync(m => m.Id == id);

    if (museum == null)
    {
      throw new InvalidOperationException("Museum not found");
    }

    return museum;
  }

  public IEnumerable<Museum> GetByIds(IEnumerable<Guid> ids)
  {
    var museums = _dbContext.Museums.Where(m => ids.Contains(m.Id)).ToList();

    // Load categories separately to avoid complex joins
    foreach (var museum in museums)
    {
      _dbContext.Entry(museum)
        .Collection(m => m.Categories)
        .Load();
    }

    return museums;
  }

  public bool IsMuseumExists(Guid id)
  {
    return _dbContext.Museums.Any(m => m.Id == id);
  }

  public bool IsMuseumNameExists(string name)
  {
    return _dbContext.Museums.Any(m => m.Name == name);
  }

  public Museum? GetByName(string name)
  {
    return _dbContext.Museums.FirstOrDefault(m => m.Name == name);
  }

  public MuseumList GetAll(MuseumQuery query)
  {
    var queryable = _dbContext.Museums.AsQueryable();
    queryable = queryable.Where(m => m.Status == MuseumStatusEnum.Active);

    if (!string.IsNullOrEmpty(query.Search))
    {
      queryable = queryable.Where(m => m.Name.Contains(query.Search) || m.Description.Contains(query.Search));
    }

    var total = queryable.Count();
    var museums = queryable
      .OrderByDescending(m => m.UpdatedAt)
      .Skip((query.Page - 1) * query.PageSize)
      .Take(query.PageSize)
      .ToList();

    // Load categories separately to avoid complex joins
    foreach (var museum in museums)
    {
      _dbContext.Entry(museum)
        .Collection(m => m.Categories)
        .Load();
    }

    return new MuseumList
    {
      Museums = museums,
      Total = total
    };
  }

  public MuseumList GetAllAdmin(MuseumQuery query)
  {
    var queryable = _dbContext.Museums.AsQueryable();

    if (!string.IsNullOrEmpty(query.Search))
    {
      queryable = queryable.Where(m => m.Name.Contains(query.Search));
    }

    if (query.Status != null)
    {
      queryable = queryable.Where(m => m.Status == query.Status);
    }

    // For category filtering, we need to use a subquery to avoid complex joins
    if (query.CategoryId != null)
    {
      var museumIdsWithCategory = _dbContext.Museums
        .Where(m => m.Categories.Any(c => c.Id == query.CategoryId))
        .Select(m => m.Id);
      queryable = queryable.Where(m => museumIdsWithCategory.Contains(m.Id));
    }

    var total = queryable.Count();
    var museums = queryable
      .OrderByDescending(m => m.UpdatedAt)
      .Skip((query.Page - 1) * query.PageSize)
      .Take(query.PageSize)
      .ToList();

    // Load categories separately to avoid complex joins
    foreach (var museum in museums)
    {
      _dbContext.Entry(museum)
        .Collection(m => m.Categories)
        .Load();
    }

    return new MuseumList
    {
      Museums = museums,
      Total = total
    };
  }

  public async Task<Museum> AddAsync(Museum museum)
  {
    var result = await _dbContext.Museums.AddAsync(museum);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<Museum> UpdateAsync(Guid id, Museum museum)
  {
    _dbContext.Museums.Update(museum);
    await _dbContext.SaveChangesAsync();
    return museum;
  }

  public async Task DeleteAsync(Museum museum)
  {
    _dbContext.Museums.Remove(museum);
    await _dbContext.SaveChangesAsync();
  }

  public async Task FeedbackMuseums(Guid museumId, int rating, Guid userId, string comment)
  {
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    try
    {
      var museum = await _dbContext.Museums.FindAsync(museumId);
      if (museum == null) throw new Exception("Museum not found");

      // find feedback of user
      var userFeedback = await _dbContext.Feedbacks
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
        await _dbContext.Feedbacks.AddAsync(newFeedback);
      }

      // save changes
      await _dbContext.SaveChangesAsync();

      // calculate average rating
      var listFeedback = await _dbContext.Feedbacks
        .Where(f => f.TargetId == museumId)
        .ToListAsync();
      var averageRating = listFeedback.Average(f => f.Rating);

      // update museum rating
      museum.Rating = (float)Math.Round(averageRating, 1);
      await _dbContext.SaveChangesAsync();
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();
      throw new InvalidOperationException("An error occurred while providing feedback for the museum.", ex);
    }
    await transaction.CommitAsync();
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
      throw new InvalidOperationException("An error occurred while providing feedback for the museum.", ex);
    }
    await transaction.CommitAsync();
  }
}