namespace Application.Service;

using Application.DTOs.Analytics;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Infrastructure.Cache;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface IAnalyticsService
{
  Task<IActionResult> GetOverview(Guid museumId);
  Task<IActionResult> GetAdminOverview();
  Task<IActionResult> GetWeeklyEventsAnalytics();
  Task<IActionResult> GetWeeklyParticipantsAnalytics(Guid museumId);
}

public class AnalyticsService : BaseService, IAnalyticsService
{
  private readonly IAnalyticsRepository _analyticsRepository;
  private readonly ICacheService _cacheService;

  public AnalyticsService(
    MuseTrip360DbContext dbContext,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    ICacheService cacheService
  )
    : base(dbContext, mapper, httpContextAccessor)
  {
    _analyticsRepository = new AnalyticsRepository(dbContext);
    _cacheService = cacheService;
  }

  public async Task<IActionResult> GetAdminOverview()
  {
    try
    {
      var overview = await _analyticsRepository.GetAdminOverview();
      return SuccessResp.Ok(overview);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> GetOverview(Guid museumId)
  {
    try
    {
      // Validate the museumId
      if (museumId == Guid.Empty)
      {
        return ErrorResp.BadRequest("Invalid museum ID.");
      }
      // get from cache
      // var cached = await _cacheService.Get<MuseumAnalyticsOverview>($"analytics:overview:{museumId}");
      // if (cached != null)
      // {
      //   return SuccessResp.Ok(cached);
      // }
      var overview = await _analyticsRepository.GetOverview(museumId);

      // Cache the overview for 1 hour
      // await _cacheService.Set($"analytics:overview:{museumId}", overview, TimeSpan.FromHours(1));
      // Return the overview
      return SuccessResp.Ok(overview);
    }
    catch (ArgumentException ex)
    {
      return ErrorResp.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> GetWeeklyEventsAnalytics()
  {
    try
    {
      var weeklyAnalytics = await _analyticsRepository.GetWeeklyEventsAnalytics();
      return SuccessResp.Ok(weeklyAnalytics);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> GetWeeklyParticipantsAnalytics(Guid museumId)
  {
    try
    {
      if (museumId == Guid.Empty)
      {
        return ErrorResp.BadRequest("Invalid museum ID.");
      }

      var weeklyParticipants = await _analyticsRepository.GetWeeklyParticipantsAnalytics(museumId);
      return SuccessResp.Ok(weeklyParticipants);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }
}