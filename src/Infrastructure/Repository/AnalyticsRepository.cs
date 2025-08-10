using Application.DTOs.Analytics;
using Database;

namespace Infrastructure.Repository;

public interface IAnalyticsRepository
{
  MuseumAnalyticsOverview GetOverview(Guid museumId);
  int GetTotalVisitors(Guid museumId);
  int GetTotalArticles(Guid museumId);
  int GetTotalEvents(Guid museumId);
  int GetTotalTourOnline(Guid museumId);
  int GetTotalFeedbacks(Guid museumId);
  int GetTotalArtifacts(Guid museumId);
}

public class AnalyticsRepository : IAnalyticsRepository
{
  private readonly MuseTrip360DbContext _context;

  public AnalyticsRepository(MuseTrip360DbContext context)
  {
    _context = context;
  }

  public MuseumAnalyticsOverview GetOverview(Guid museumId)
  {
    var museum = _context.Museums.Find(museumId);
    if (museum == null)
    {
      throw new ArgumentException("Museum not found", nameof(museumId));
    }


    return new MuseumAnalyticsOverview
    {
      TotalVisitors = GetTotalVisitors(museumId),
      TotalArticles = GetTotalArticles(museumId),
      TotalEvents = GetTotalEvents(museumId),
      TotalTourOnlines = GetTotalTourOnline(museumId),
      AverageRating = museum.Rating,
      TotalFeedbacks = GetTotalFeedbacks(museumId),
      TotalArtifacts = GetTotalArtifacts(museumId),
    };
  }

  public int GetTotalArticles(Guid museumId)
  {
    // Assuming you have a method to get the total articles for a museum
    return _context.Articles.Count(a => a.MuseumId == museumId);
  }

  public int GetTotalEvents(Guid museumId)
  {
    // Assuming you have a method to get the total events for a museum
    return _context.Events.Count(e => e.MuseumId == museumId);
  }

  public int GetTotalFeedbacks(Guid museumId)
  {
    return 0;
  }

  public int GetTotalTourOnline(Guid museumId)
  {
    // Assuming you have a method to get the total online tours for a museum
    return _context.TourOnlines.Count(t => t.MuseumId == museumId);
  }

  public int GetTotalVisitors(Guid museumId)
  {
    return 0;
  }

  public int GetTotalArtifacts(Guid museumId)
  {
    return _context.Artifacts.Count(a => a.MuseumId == museumId);
  }
}