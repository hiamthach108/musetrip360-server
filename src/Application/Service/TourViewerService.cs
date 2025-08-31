using Application.Service;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface ITourViewerService
{
    Task<IActionResult> HandleGetByIdAsync(Guid id);
    Task<IActionResult> HandleGetAllAsync();
    Task<IActionResult> HandleGetByTourIdAsync(Guid tourId);
    Task<IActionResult> HandleGetByUserIdAsync(Guid userId);
    Task<IActionResult> HandleGetByTourIdAndUserIdAsync(Guid tourId, Guid userId);
    Task<IActionResult> HandleGetActiveTourViewersAsync(Guid tourId);
}

public class TourViewerService : BaseService, ITourViewerService
{
    private readonly ITourViewerRepository _tourViewerRepository;
    public TourViewerService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(dbContext, mapper, httpContextAccessor)
    {
        _tourViewerRepository = new TourViewerRepository(dbContext);
    }
    public async Task<IActionResult> HandleGetActiveTourViewersAsync(Guid tourId)
    {
        try
        {
            var tourViewers = await _tourViewerRepository.GetActiveTourViewersAsync(tourId);
            return SuccessResp.Ok(_mapper.Map<IEnumerable<TourViewerDto>>(tourViewers));
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAllAsync()
    {
        try
        {
            var tourViewers = await _tourViewerRepository.GetAllAsync();
            return SuccessResp.Ok(_mapper.Map<IEnumerable<TourViewerDto>>(tourViewers));
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
            var tourViewer = await _tourViewerRepository.GetByIdAsync(id);
            if (tourViewer == null)
            {
                return ErrorResp.NotFound("Tour viewer not found");
            }
            return SuccessResp.Ok(_mapper.Map<TourViewerDto>(tourViewer));
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetByTourIdAndUserIdAsync(Guid tourId, Guid userId)
    {
        try
        {
            var tourViewer = await _tourViewerRepository.GetByTourIdAndUserIdAsync(tourId, userId);
            if (tourViewer == null)
            {
                return ErrorResp.NotFound("Tour viewer not found");
            }
            return SuccessResp.Ok(_mapper.Map<TourViewerDto>(tourViewer));
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetByTourIdAsync(Guid tourId)
    {
        try
        {
            var tourViewers = await _tourViewerRepository.GetByTourIdAsync(tourId);
            return SuccessResp.Ok(_mapper.Map<IEnumerable<TourViewerDto>>(tourViewers));
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetByUserIdAsync(Guid userId)
    {
        try
        {
            var tourViewers = await _tourViewerRepository.GetByUserIdAsync(userId);
            return SuccessResp.Ok(_mapper.Map<IEnumerable<TourViewerDto>>(tourViewers));
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}