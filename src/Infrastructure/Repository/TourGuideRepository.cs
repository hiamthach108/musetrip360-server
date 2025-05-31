using Database;
using Domain.Tours;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public interface ITourGuideRepository
{
  Task<TourGuideList> GetAllTourGuidesAsync(TourGuideQuery query);
  Task<TourGuide?> GetTourGuideByIdAsync(Guid id);
  Task<IEnumerable<TourGuide>> GetTourGuideByUserIdAsync(Guid userId);
  Task<IEnumerable<TourGuide>> GetTourGuideByMuseumIdAsync(Guid museumId);
  Task<IEnumerable<TourGuide>> GetTourGuideByEventIdAsync(Guid eventId);
  Task AddTourGuideAsync(TourGuide tourGuide);
  Task UpdateTourGuideAsync(TourGuide tourGuide);
  Task DeleteTourGuideAsync(Guid id);
  Task<bool> IsTourGuideExistsAsync(Guid id);
  Task<TourGuideListWithMissingIds> GetTourGuideByListIdEventIdStatus(IEnumerable<Guid> tourGuideIds, Guid eventId, bool isAvailable);
  Task<TourGuideListWithMissingIds> GetTourGuideByListId(IEnumerable<Guid> tourGuideIds);
}

public class TourGuideListWithMissingIds
{
  public IEnumerable<TourGuide> TourGuides { get; set; } = [];
  public bool IsAllFound { get; set; }
  public IEnumerable<Guid> MissingIds { get; set; } = [];
}
public class TourGuideList
{
  public IEnumerable<TourGuide> TourGuides { get; set; } = [];
  public int Total { get; set; }
}

public class TourGuideRepository : ITourGuideRepository
{
  private readonly MuseTrip360DbContext _context;
  public TourGuideRepository(MuseTrip360DbContext context)
  {
    _context = context;
  }

  public async Task AddTourGuideAsync(TourGuide tourGuide)
  {
    await _context.TourGuides.AddAsync(tourGuide);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteTourGuideAsync(Guid id)
  {
    var tourGuide = await _context.TourGuides.FindAsync(id);
    if (tourGuide != null)
    {
      _context.TourGuides.Remove(tourGuide);
      await _context.SaveChangesAsync();
    }
  }

  public async Task<TourGuideList> GetAllTourGuidesAsync(TourGuideQuery query)
  {
    var queryable = _context.TourGuides
    .Where(t => query.Name == null || t.Name.Contains(query.Name))
    .Where(t => query.Bio == null || t.Bio.Contains(query.Bio))
    .Where(t => query.IsAvailable == null || t.IsAvailable == query.IsAvailable)
    .Where(t => query.MuseumId == null || t.MuseumId == query.MuseumId)
    .Where(t => query.UserId == null || t.UserId == query.UserId)
    .Where(t => query.EventId == null || t.Events.Any(e => e.Id == query.EventId));

    var total = await queryable.CountAsync();
    var tourGuides = await queryable
    .Skip((query.Page - 1) * query.PageSize)
    .Take(query.PageSize)
    .ToListAsync();

    return new TourGuideList { TourGuides = tourGuides, Total = total };
  }

  public async Task<TourGuide?> GetTourGuideByIdAsync(Guid id)
  {
    return await _context.TourGuides.FindAsync(id);
  }

  public async Task<TourGuideListWithMissingIds> GetTourGuideByListId(IEnumerable<Guid> tourGuideIds)
  {
    var tourGuides = await _context.TourGuides.Where(t => tourGuideIds.Contains(t.Id)).ToListAsync();
    var missingIds = tourGuideIds.Except(tourGuides.Select(t => t.Id)).ToList();
    var allFound = missingIds.Count == 0;
    return new TourGuideListWithMissingIds { TourGuides = tourGuides, IsAllFound = allFound, MissingIds = missingIds };
  }

  public async Task<TourGuideListWithMissingIds> GetTourGuideByListIdEventIdStatus(IEnumerable<Guid> tourGuideIds, Guid eventId, bool isAvailable)
  {
    var tourGuideIdsList = tourGuideIds.ToList();
    var tourGuides = await _context.TourGuides
    .Where(t => tourGuideIdsList.Contains(t.Id))
    .Where(t => t.Events.Any(e => e.Id == eventId))
    .Where(t => t.IsAvailable == isAvailable)
    .ToListAsync();

    var missingIds = tourGuideIdsList.Except(tourGuides.Select(t => t.Id)).ToList();
    var allFound = missingIds.Count == 0;
    return new TourGuideListWithMissingIds { TourGuides = tourGuides, IsAllFound = allFound, MissingIds = missingIds };
  }

  public async Task<IEnumerable<TourGuide>> GetTourGuideByMuseumIdAsync(Guid museumId)
  {
    return await _context.TourGuides.Where(t => t.MuseumId == museumId).ToListAsync();
  }

  public async Task<IEnumerable<TourGuide>> GetTourGuideByEventIdAsync(Guid eventId)
  {
    return await _context.TourGuides.Where(t => t.Events.Any(e => e.Id == eventId)).ToListAsync();
  }

  public async Task<IEnumerable<TourGuide>> GetTourGuideByUserIdAsync(Guid userId)
  {
    return await _context.TourGuides.Where(t => t.UserId == userId).ToListAsync();
  }

  public async Task<bool> IsTourGuideExistsAsync(Guid id)
  {
    return await _context.TourGuides.AnyAsync(t => t.Id == id);
  }

  public async Task UpdateTourGuideAsync(TourGuide tourGuide)
  {
    var existingTourGuide = await _context.TourGuides.FindAsync(tourGuide.Id);
    if (existingTourGuide != null)
    {
      _context.Entry(existingTourGuide).CurrentValues.SetValues(tourGuide);
      await _context.SaveChangesAsync();
    }
  }
}