using Application.Service;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Tours;
using Microsoft.AspNetCore.Mvc;

public interface ITourContentService
{
    Task<IActionResult> HandleGetAllAsync(TourContentQuery query);
    Task<IActionResult> HandleGetByIdAsync(Guid id);
}
public interface IAdminTourContentService : ITourContentService
{
    Task<IActionResult> HandleGetAllAdminAsync(TourContentAdminQuery query);
    Task<IActionResult> HandleCreateAsync(Guid tourOnlineId, TourContentCreateDto tourContent);
    Task<IActionResult> HandleUpdateAsync(Guid id, TourContentUpdateDto tourContent);
    Task<IActionResult> HandleDeleteAsync(Guid id);
    Task<IActionResult> HandleActivateAsync(Guid id);
    Task<IActionResult> HandleDeactivateAsync(Guid id);
    Task<IActionResult> HandleGetByTourOnlineIdAsync(Guid tourOnlineId, TourContentAdminQuery query);
}

public abstract class BaseTourContentService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService(dbContext, mapper, httpContextAccessor), ITourContentService
{
    protected readonly ITourContentRepository _tourContentRepository = new TourContentRepository(dbContext);
    protected readonly ITourOnlineRepository _tourOnlineRepository = new TourOnlineRepository(dbContext);
    public async Task<IActionResult> HandleGetAllAsync(TourContentQuery query)
    {
        try
        {
            var tourContents = await _tourContentRepository.GetTourContents(query);
            var tourContentDtos = _mapper.Map<List<TourContentDto>>(tourContents.Contents);
            return SuccessResp.Ok(new { List = tourContentDtos, Total = tourContents.Total });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
    public async Task<IActionResult> HandleGetByIdAsync(Guid id)
    {
        try
        {
            var tourContent = await _tourContentRepository.GetTourContentById(id);
            if (tourContent == null)
            {
                return ErrorResp.NotFound("Tour content not found");
            }
            var tourContentDto = _mapper.Map<TourContentDto>(tourContent);
            return SuccessResp.Ok(tourContentDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}
public class TourContentService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseTourContentService(dbContext, mapper, httpContextAccessor), ITourContentService
{
}
public class AdminTourContentService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseTourContentService(dbContext, mapper, httpContextAccessor), IAdminTourContentService
{
    public async Task<IActionResult> HandleActivateAsync(Guid id)
    {
        try
        {
            var tourContent = await _tourContentRepository.GetTourContentById(id);
            if (tourContent == null)
            {
                return ErrorResp.NotFound("Tour content not found");
            }
            tourContent.IsActive = true;
            await _tourContentRepository.UpdateTourContent(id, tourContent);
            return SuccessResp.Ok(tourContent);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleCreateAsync(Guid tourOnlineId, TourContentCreateDto tourContent)
    {
        try
        {
            var tourOnline = await _tourOnlineRepository.IsTourOnlineExists(tourOnlineId);
            if (!tourOnline)
            {
                return ErrorResp.NotFound("Tour online not found");
            }
            var tourContentMapped = _mapper.Map<TourContent>(tourContent);
            tourContentMapped.TourId = tourOnlineId;
            await _tourContentRepository.CreateTourContent(tourContentMapped);
            return SuccessResp.Ok(tourContentMapped);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleDeactivateAsync(Guid id)
    {
        try
        {
            var tourContent = await _tourContentRepository.GetTourContentById(id);
            if (tourContent == null)
            {
                return ErrorResp.NotFound("Tour content not found");
            }
            tourContent.IsActive = false;
            await _tourContentRepository.UpdateTourContent(id, tourContent);
            return SuccessResp.Ok(tourContent);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleDeleteAsync(Guid id)
    {
        try
        {
            var tourContent = await _tourContentRepository.GetTourContentById(id);
            if (tourContent == null)
            {
                return ErrorResp.NotFound("Tour content not found");
            }
            await _tourContentRepository.DeleteTourContent(id);
            return SuccessResp.Ok("Tour content deleted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAllAdminAsync(TourContentAdminQuery query)
    {
        try
        {
            var tourContents = await _tourContentRepository.GetTourContents(query);
            var tourContentDtos = _mapper.Map<List<TourContentDto>>(tourContents.Contents);
            return SuccessResp.Ok(new { List = tourContentDtos, Total = tourContents.Total });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetByTourOnlineIdAsync(Guid tourOnlineId, TourContentAdminQuery query)
    {
        try
        {
            var tourContents = await _tourContentRepository.GetTourContentsByTourOnlineId(query, tourOnlineId);
            var tourContentDtos = _mapper.Map<List<TourContentDto>>(tourContents.Contents);
            return SuccessResp.Ok(new { List = tourContentDtos, Total = tourContents.Total });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleUpdateAsync(Guid id, TourContentUpdateDto tourContent)
    {
        try
        {
            var tourContentExisted = await _tourContentRepository.GetTourContentById(id);
            if (tourContentExisted == null)
            {
                return ErrorResp.NotFound("Tour content not found");
            }
            var tourContentMapped = _mapper.Map(tourContent, tourContentExisted);
            await _tourContentRepository.UpdateTourContent(id, tourContentMapped);
            return SuccessResp.Ok(tourContentMapped);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}
