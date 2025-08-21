using Application.DTOs.Analytics;
using Application.Shared.Enum;
using Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public interface IAnalyticsRepository
{
  Task<MuseumAnalyticsOverview> GetOverview(Guid museumId);
  Task<AdminAnalyticsOverview> GetAdminOverview();
  Task<int> GetTotalVisitors(Guid museumId);
  Task<int> GetTotalArticles(Guid museumId);
  Task<int> GetTotalEvents(Guid museumId);
  Task<int> GetTotalTourOnline(Guid museumId);
  Task<int> GetTotalFeedbacks(Guid museumId);
  Task<int> GetTotalArtifacts(Guid museumId);
  Task<double> GetTotalRevenue(Guid museumId);
  Task<double> GetAverageRating(Guid museumId);
}

public class AnalyticsRepository : IAnalyticsRepository
{
  private readonly MuseTrip360DbContext _context;

  public AnalyticsRepository(MuseTrip360DbContext context)
  {
    _context = context;
  }

  public async Task<MuseumAnalyticsOverview> GetOverview(Guid museumId)
  {
    var overview = new MuseumAnalyticsOverview
    {
      TotalVisitors = await GetTotalVisitors(museumId),
      TotalArticles = await GetTotalArticles(museumId),
      TotalEvents = await GetTotalEvents(museumId),
      TotalTourOnlines = await GetTotalTourOnline(museumId),
      AverageRating = await GetAverageRating(museumId),
      TotalFeedbacks = await GetTotalFeedbacks(museumId),
      TotalRevenue = await GetTotalRevenue(museumId),
      TotalArtifacts = await GetTotalArtifacts(museumId),
    };

    return overview;
  }

  public async Task<int> GetTotalArticles(Guid museumId)
  {
    // Assuming you have a method to get the total articles for a museum
    return await _context.Articles.CountAsync(a => a.MuseumId == museumId);
  }

  public async Task<int> GetTotalEvents(Guid museumId)
  {
    // Assuming you have a method to get the total events for a museum
    return await _context.Events.CountAsync(e => e.MuseumId == museumId);
  }

  public async Task<int> GetTotalFeedbacks(Guid museumId)
  {
    return await _context.Feedbacks.CountAsync(f => f.TargetId == museumId);
  }

  public async Task<int> GetTotalTourOnline(Guid museumId)
  {
    // Assuming you have a method to get the total online tours for a museum
    return await _context.TourOnlines.CountAsync(t => t.MuseumId == museumId);
  }

  public async Task<int> GetTotalVisitors(Guid museumId)
  {
    return await _context.EventParticipants.CountAsync(e => e.Event.MuseumId == museumId);
  }

  public async Task<int> GetTotalArtifacts(Guid museumId)
  {
    return await _context.Artifacts.CountAsync(a => a.MuseumId == museumId);
  }
  public async Task<double> GetTotalRevenue(Guid museumId)
  {
    var tourRevenue = await _context.TourOnlines
    .Where(t => t.MuseumId == museumId)
    .SelectMany(t => t.OrderTours
    .SelectMany(ot => ot.Order.Payments
    .Select(p => p.Amount)))
    .SumAsync();

    var eventRevenue = await _context.Events
    .Where(e => e.MuseumId == museumId)
    .SelectMany(e => e.OrderEvents
    .SelectMany(oe => oe.Order.Payments
    .Select(p => p.Amount)))
    .SumAsync();

    return (double)(tourRevenue + eventRevenue);
  }

  public async Task<double> GetAverageRating(Guid museumId)
  {
    var feedbacks = await _context.Feedbacks.Where(f => f.TargetId == museumId).ToListAsync();
    if (feedbacks.Count == 0)
    {
      return 0;
    }
    return feedbacks.Average(f => f.Rating);
  }

  public async Task<AdminAnalyticsOverview> GetAdminOverview()
  {
    return new AdminAnalyticsOverview
    {
      TotalMuseums = await _context.Museums.CountAsync(),
      TotalPendingRequests = await _context.MuseumRequests.Where(r => r.Status == RequestStatusEnum.Pending).CountAsync(),
      TotalUsers = await _context.Users.CountAsync(),
      TotalEvents = await _context.Events.CountAsync(),
      TotalTours = await _context.TourOnlines.CountAsync(),
      MuseumsByCategory = [.. _context.Museums
        .SelectMany(m => m.Categories, (museum, category) => new { museum, category })
        .GroupBy(x => x.category)
        .Select(g => new AnalyticsMuseumCategorize
        {
          Category = g.Key.Name,
          Count = g.Count()
        })]
    };
  }

}