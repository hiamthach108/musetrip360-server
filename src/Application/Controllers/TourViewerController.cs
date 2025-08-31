namespace MuseTrip360.Controllers;

using Application.Middlewares;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for managing tour viewer operations
/// </summary>
[ApiController]
[Route("/api/v1/tour-viewers")]
public class TourViewerController : ControllerBase
{
    private readonly ILogger<TourViewerController> _logger;
    private readonly ITourViewerService _service;

    /// <summary>
    /// Initializes a new instance of the TourViewerController
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="service">The tour viewer service</param>
    public TourViewerController(ILogger<TourViewerController> logger, ITourViewerService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Gets all tour viewers
    /// </summary>
    /// <returns>A list of all tour viewers</returns>
    /// <response code="200">Returns the list of tour viewers</response>
    /// <response code="500">If there was an internal server error</response>
    [Protected]
    [HttpGet("")]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Get all tour viewers request received");
        return await _service.HandleGetAllAsync();
    }

    /// <summary>
    /// Gets a specific tour viewer by ID
    /// </summary>
    /// <param name="id">The unique identifier of the tour viewer</param>
    /// <returns>The tour viewer with the specified ID</returns>
    /// <response code="200">Returns the tour viewer</response>
    /// <response code="404">If the tour viewer is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [Protected]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Get tour viewer by ID request received: {Id}", id);
        return await _service.HandleGetByIdAsync(id);
    }

    /// <summary>
    /// Gets all tour viewers for a specific tour
    /// </summary>
    /// <param name="tourId">The unique identifier of the tour</param>
    /// <returns>A list of tour viewers for the specified tour</returns>
    /// <response code="200">Returns the list of tour viewers for the tour</response>
    /// <response code="500">If there was an internal server error</response>
    [Protected]
    [HttpGet("tour/{tourId}")]
    public async Task<IActionResult> GetByTourId(Guid tourId)
    {
        _logger.LogInformation("Get tour viewers by tour ID request received: {TourId}", tourId);
        return await _service.HandleGetByTourIdAsync(tourId);
    }

    /// <summary>
    /// Gets all tour viewers for a specific user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>A list of tour viewers for the specified user</returns>
    /// <response code="200">Returns the list of tour viewers for the user</response>
    /// <response code="500">If there was an internal server error</response>
    [Protected]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        _logger.LogInformation("Get tour viewers by user ID request received: {UserId}", userId);
        return await _service.HandleGetByUserIdAsync(userId);
    }

    /// <summary>
    /// Gets a specific tour viewer by tour ID and user ID
    /// </summary>
    /// <param name="tourId">The unique identifier of the tour</param>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>The tour viewer for the specified tour and user</returns>
    /// <response code="200">Returns the tour viewer</response>
    /// <response code="404">If the tour viewer is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [Protected]
    [HttpGet("tour/{tourId}/user/{userId}")]
    public async Task<IActionResult> GetByTourIdAndUserId(Guid tourId, Guid userId)
    {
        _logger.LogInformation("Get tour viewer by tour ID and user ID request received: TourId={TourId}, UserId={UserId}", tourId, userId);
        return await _service.HandleGetByTourIdAndUserIdAsync(tourId, userId);
    }

    /// <summary>
    /// Gets all active tour viewers for a specific tour
    /// </summary>
    /// <param name="tourId">The unique identifier of the tour</param>
    /// <returns>A list of active tour viewers for the specified tour</returns>
    /// <response code="200">Returns the list of active tour viewers for the tour</response>
    /// <response code="500">If there was an internal server error</response>
    [Protected]
    [HttpGet("tour/{tourId}/active")]
    public async Task<IActionResult> GetActiveTourViewers(Guid tourId)
    {
        _logger.LogInformation("Get active tour viewers by tour ID request received: {TourId}", tourId);
        return await _service.HandleGetActiveTourViewersAsync(tourId);
    }
}
