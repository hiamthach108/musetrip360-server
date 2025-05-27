using Application.Service;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Tours;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface ITourGuideService
{
  Task<IActionResult> GetTourGuideByIdAsync(Guid id);
  Task<IActionResult> GetTourGuideByEventIdAsync(Guid eventId);
  Task<IActionResult> GetTourGuideByMuseumIdAsync(Guid museumId);
  Task<IActionResult> GetTourGuideByUserIdAsync(Guid userId);
  Task<IActionResult> GetTourGuideByQueryAsync(TourGuideQuery query);
}
public interface IAdminTourGuideService : ITourGuideService
{
  Task<IActionResult> CreateTourGuideAsync(TourGuideCreateDto tourGuideCreateDto, Guid museumId);
  Task<IActionResult> UpdateTourGuideAsync(Guid id, TourGuideUpdateDto tourGuideUpdateDto);
  Task<IActionResult> DeleteTourGuideAsync(Guid id);
}

public abstract class BaseTourGuideService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService(context, mapper, httpContextAccessor), ITourGuideService
{
  protected readonly ITourGuideRepository _tourGuideRepository = new TourGuideRepository(context);
  public async Task<IActionResult> GetTourGuideByEventIdAsync(Guid eventId)
  {
    try
    {

      var tourGuide = await _tourGuideRepository.GetTourGuideByEventIdAsync(eventId);
      var tourGuideDtos = _mapper.Map<IEnumerable<TourGuideDto>>(tourGuide);
      return SuccessResp.Ok(tourGuideDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> GetTourGuideByIdAsync(Guid id)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetTourGuideByIdAsync(id);
      var tourGuideDtos = _mapper.Map<TourGuideDto>(tourGuide);
      return SuccessResp.Ok(tourGuideDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> GetTourGuideByMuseumIdAsync(Guid museumId)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetTourGuideByMuseumIdAsync(museumId);
      var tourGuideDtos = _mapper.Map<IEnumerable<TourGuideDto>>(tourGuide);
      return SuccessResp.Ok(tourGuideDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> GetTourGuideByQueryAsync(TourGuideQuery query)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetAllTourGuidesAsync(query);
      var tourGuideDtos = _mapper.Map<IEnumerable<TourGuideDto>>(tourGuide.TourGuides);
      return SuccessResp.Ok(tourGuideDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError(ex.Message);
    }
  }

  public async Task<IActionResult> GetTourGuideByUserIdAsync(Guid userId)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetTourGuideByUserIdAsync(userId);
      var tourGuideDtos = _mapper.Map<IEnumerable<TourGuideDto>>(tourGuide);
      return SuccessResp.Ok(tourGuideDtos);
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
  public async Task<IActionResult> CreateTourGuideAsync(TourGuideCreateDto tourGuideCreateDto, Guid museumId)
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

  public async Task<IActionResult> DeleteTourGuideAsync(Guid id)
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

  public async Task<IActionResult> UpdateTourGuideAsync(Guid id, TourGuideUpdateDto tourGuideUpdateDto)
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
