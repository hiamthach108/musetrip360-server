using Application.Service;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Tours;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface ITourGuideService
{
  Task<IActionResult> HandleGetTourGuideByIdAsync(Guid id);
  Task<IActionResult> HandleGetTourGuideByEventIdAsync(Guid eventId, TourGuideQuery query);
  Task<IActionResult> HandleGetTourGuideByMuseumIdAsync(Guid museumId, TourGuideQuery query);
  Task<IActionResult> HandleGetTourGuideByUserIdAsync(Guid userId, TourGuideQuery query);
  Task<IActionResult> HandleGetTourGuideByQueryAsync(TourGuideQuery query);
}
public interface IAdminTourGuideService : ITourGuideService
{
  Task<IActionResult> HandleCreateTourGuideAsync(TourGuideCreateDto tourGuideCreateDto, Guid museumId);
  Task<IActionResult> HandleUpdateTourGuideAsync(Guid id, TourGuideUpdateDto tourGuideUpdateDto);
  Task<IActionResult> HandleDeleteTourGuideAsync(Guid id);
  Task<IActionResult> HandleAvailableTourGuideAsync(Guid id, bool isAvailable);
}

public abstract class BaseTourGuideService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService(context, mapper, httpContextAccessor), ITourGuideService
{
  protected readonly ITourGuideRepository _tourGuideRepository = new TourGuideRepository(context);
  public async Task<IActionResult> HandleGetTourGuideByEventIdAsync(Guid eventId, TourGuideQuery query)
  {
    try
    {

      var tourGuide = await _tourGuideRepository.GetTourGuideByEventIdAsync(query, eventId);
      var tourGuideDtos = _mapper.Map<IEnumerable<TourGuideDto>>(tourGuide.TourGuides);
      return SuccessResp.Ok(new
      {
        List = tourGuideDtos,
        Total = tourGuide.Total
      });
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> HandleGetTourGuideByIdAsync(Guid id)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetTourGuideByIdAsync(id);
      if (tourGuide == null)
      {
        return ErrorResp.NotFound("Tour guide not found");
      }
      var tourGuideDtos = _mapper.Map<TourGuideDto>(tourGuide);
      return SuccessResp.Ok(tourGuideDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> HandleGetTourGuideByMuseumIdAsync(Guid museumId, TourGuideQuery query)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetTourGuideByMuseumIdAsync(query, museumId);
      var tourGuideDtos = _mapper.Map<IEnumerable<TourGuideDto>>(tourGuide.TourGuides);
      return SuccessResp.Ok(new
      {
        List = tourGuideDtos,
        Total = tourGuide.Total
      });
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> HandleGetTourGuideByQueryAsync(TourGuideQuery query)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetAllTourGuidesAsync(query);
      var tourGuideDtos = _mapper.Map<IEnumerable<TourGuideDto>>(tourGuide.TourGuides);
      return SuccessResp.Ok(new
      {
        List = tourGuideDtos,
        Total = tourGuide.Total
      });
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> HandleGetTourGuideByUserIdAsync(Guid userId, TourGuideQuery query)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetTourGuideByUserIdAsync(query, userId);
      var tourGuideDtos = _mapper.Map<IEnumerable<TourGuideDto>>(tourGuide.TourGuides);
      return SuccessResp.Ok(new
      {
        List = tourGuideDtos,
        Total = tourGuide.Total
      });
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

}

public class TourGuideService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseTourGuideService(context, mapper, httpContextAccessor), ITourGuideService
{
}

public class AdminTourGuideService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseTourGuideService(context, mapper, httpContextAccessor), IAdminTourGuideService
{
  protected readonly IMuseumRepository _museumRepository = new MuseumRepository(context);

  public async Task<IActionResult> HandleAvailableTourGuideAsync(Guid id, bool isAvailable)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetTourGuideByIdAsync(id);
      if (tourGuide == null)
      {
        return ErrorResp.NotFound("Tour guide not found");
      }
      tourGuide.IsAvailable = isAvailable;
      await _tourGuideRepository.UpdateTourGuideAsync(tourGuide);
      var tourGuideDtos = _mapper.Map<TourGuideDto>(tourGuide);
      return SuccessResp.Ok(tourGuideDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> HandleCreateTourGuideAsync(TourGuideCreateDto tourGuideCreateDto, Guid museumId)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Unauthorized");
      }
      var tourGuide = _mapper.Map<TourGuide>(tourGuideCreateDto);
      tourGuide.UserId = payload.UserId;
      var museum = _museumRepository.GetById(museumId);
      if (museum == null)
      {
        return ErrorResp.NotFound("Museum not found");
      }
      tourGuide.MuseumId = museumId;
      await _tourGuideRepository.AddTourGuideAsync(tourGuide);
      var tourGuideDtos = _mapper.Map<TourGuideDto>(tourGuide);
      return SuccessResp.Ok(tourGuideDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> HandleDeleteTourGuideAsync(Guid id)
  {
    try
    {
      await _tourGuideRepository.DeleteTourGuideAsync(id);
      return SuccessResp.Ok(new { message = "Tour guide deleted successfully" });
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> HandleUpdateTourGuideAsync(Guid id, TourGuideUpdateDto tourGuideUpdateDto)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetTourGuideByIdAsync(id);
      if (tourGuide == null)
      {
        return ErrorResp.NotFound("Tour guide not found");
      }
      _mapper.Map(tourGuideUpdateDto, tourGuide);
      await _tourGuideRepository.UpdateTourGuideAsync(tourGuide);
      var tourGuideDtos = _mapper.Map<TourGuideDto>(tourGuide);
      return SuccessResp.Ok(tourGuideDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }
}
