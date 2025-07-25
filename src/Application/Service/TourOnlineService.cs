using Application.Service;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Tours;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface ITourOnlineService
{
    Task<IActionResult> GetAllAsync(TourOnlineQuery query);
    Task<IActionResult> GetByIdAsync(Guid id);
}

public interface IAdminTourOnlineService : ITourOnlineService
{
    Task<IActionResult> CreateAsync(Guid museumId, TourOnlineCreateDto tourOnline);
    Task<IActionResult> UpdateAsync(Guid id, TourOnlineUpdateDto tourOnline);
    Task<IActionResult> DeleteAsync(Guid id);
    Task<IActionResult> ActivateAsync(Guid id);
    Task<IActionResult> DeactivateAsync(Guid id);
    Task<IActionResult> GetAllAdminAsync(TourOnlineAdminQuery query);
    Task<IActionResult> GetAllByMuseumIdAsync(Guid museumId, TourOnlineAdminQuery query);
    Task<IActionResult> AddTourContentToTourAsync(Guid tourOnlineId, IEnumerable<Guid> tourContentIds);
    Task<IActionResult> RemoveTourContentFromTourAsync(Guid tourOnlineId, IEnumerable<Guid> tourContentIds);
}

public abstract class BaseTourOnlineService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService(dbContext, mapper, httpContextAccessor), ITourOnlineService
{
    protected readonly ITourOnlineRepository _tourOnlineRepository = new TourOnlineRepository(dbContext);
    protected readonly IMuseumRepository _museumRepository = new MuseumRepository(dbContext);
    protected readonly ITourContentRepository _tourContentRepository = new TourContentRepository(dbContext);
    public virtual async Task<IActionResult> GetAllAsync(TourOnlineQuery query)
    {
        try
        {
            var tours = await _tourOnlineRepository.GetAllAsync(query);
            var tourDtos = _mapper.Map<List<TourOnlineDto>>(tours.Tours);
            return SuccessResp.Ok(new { List = tourDtos, Total = tours.Total });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
    public virtual async Task<IActionResult> GetByIdAsync(Guid id)
    {
        try
        {
            var tour = await _tourOnlineRepository.GetByIdAsync(id);
            var tourDto = _mapper.Map<TourOnlineDto>(tour);
            return SuccessResp.Ok(tourDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}

public class TourOnlineService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseTourOnlineService(dbContext, mapper, httpContextAccessor), ITourOnlineService
{
}

public class TourOnlineAdminService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseTourOnlineService(dbContext, mapper, httpContextAccessor), IAdminTourOnlineService
{
    public async Task<IActionResult> ActivateAsync(Guid id)
    {
        try
        {
            var tour = await _tourOnlineRepository.GetByIdAsync(id);
            if (tour == null)
            {
                return ErrorResp.NotFound("Tour not found");
            }
            tour.IsActive = true;
            await _tourOnlineRepository.UpdateAsync(tour);
            var tourDto = _mapper.Map<TourOnlineDto>(tour);
            return SuccessResp.Ok(tour);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> AddTourContentToTourAsync(Guid tourOnlineId, IEnumerable<Guid> tourContentIds)
    {
        try
        {
            var tour = await _tourOnlineRepository.GetByIdAsync(tourOnlineId);
            if (tour == null)
            {
                return ErrorResp.NotFound("Tour not found");
            }
            var tourContentList = await _tourContentRepository.GetTourContentsByListIdTourOnlineIdStatus(tourContentIds, tourOnlineId, true);
            if (!tourContentList.IsAllFound)
            {
                return ErrorResp.BadRequest($"Some tour contents not found or not active: {string.Join(", ", tourContentList.MissingIds)}");
            }
            foreach (var tourContent in tourContentList.Contents)
            {
                tour.TourContents.Add(tourContent);
            }
            await _tourOnlineRepository.UpdateAsync(tour);
            return SuccessResp.Ok(new { Message = "Tour contents added to tour successfully" });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> CreateAsync(Guid museumId, TourOnlineCreateDto tourOnline)
    {
        try
        {
            var museum = _museumRepository.GetById(museumId);
            if (museum == null)
            {
                return ErrorResp.NotFound("Museum not found");
            }
            var tour = _mapper.Map<TourOnline>(tourOnline);
            tour.MuseumId = museumId;
            await _tourOnlineRepository.CreateAsync(tour);
            var tourDto = _mapper.Map<TourOnlineDto>(tour);
            return SuccessResp.Created(tourDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> DeactivateAsync(Guid id)
    {
        try
        {
            var tour = await _tourOnlineRepository.GetByIdAsync(id);
            if (tour == null)
            {
                return ErrorResp.NotFound("Tour not found");
            }
            tour.IsActive = false;
            await _tourOnlineRepository.UpdateAsync(tour);
            var tourDto = _mapper.Map<TourOnlineDto>(tour);
            return SuccessResp.Ok(tour);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var isTourOnlineExists = await _tourOnlineRepository.IsTourOnlineExists(id);
            if (!isTourOnlineExists)
            {
                return ErrorResp.NotFound("Tour not found");
            }
            await _tourOnlineRepository.DeleteAsync(id);
            return SuccessResp.Ok(new { Message = "Tour deleted successfully" });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
    public async Task<IActionResult> GetAllAdminAsync(TourOnlineAdminQuery query)
    {
        try
        {
            var tours = await _tourOnlineRepository.GetAllAdminAsync(query);
            var tourDtos = _mapper.Map<List<TourOnlineDto>>(tours.Tours);
            return SuccessResp.Ok(new { List = tourDtos, Total = tours.Total });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> GetAllByMuseumIdAsync(Guid museumId, TourOnlineAdminQuery query)
    {
        try
        {
            var tours = await _tourOnlineRepository.GetAllByMuseumIdAsync(museumId, query);
            var tourDtos = _mapper.Map<List<TourOnlineDto>>(tours.Tours);
            return SuccessResp.Ok(new { List = tourDtos, Total = tours.Total });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> RemoveTourContentFromTourAsync(Guid tourOnlineId, IEnumerable<Guid> tourContentIds)
    {
        try
        {
            var tour = await _tourOnlineRepository.GetByIdAsync(tourOnlineId);
            if (tour == null)
            {
                return ErrorResp.NotFound("Tour not found");
            }
            var tourContentList = await _tourContentRepository.GetTourContentsByListIdTourOnlineIdStatus(tourContentIds, tourOnlineId, true);
            if (!tourContentList.IsAllFound)
            {
                return ErrorResp.BadRequest($"Some tour contents not found or not active: {string.Join(", ", tourContentList.MissingIds)}");
            }
            foreach (var tourContent in tourContentList.Contents)
            {
                tour.TourContents.Remove(tourContent);
            }
            await _tourOnlineRepository.UpdateAsync(tour);
            return SuccessResp.Ok(new { Message = "Tour contents removed from tour successfully" });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> UpdateAsync(Guid id, TourOnlineUpdateDto tourOnline)
    {
        try
        {
            var tour = await _tourOnlineRepository.GetByIdAsync(id);
            if (tour == null)
            {
                return ErrorResp.NotFound("Tour not found");
            }
            _mapper.Map(tourOnline, tour);
            await _tourOnlineRepository.UpdateAsync(tour);
            var tourDto = _mapper.Map<TourOnlineDto>(tour);
            return SuccessResp.Ok(tour);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}