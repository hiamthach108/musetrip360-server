using Application.Middlewares;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for managing tour guide operations
/// </summary>
[Route("api/v1/tour-guides")]
[ApiController]
[Produces("application/json")]
public class TourGuideController : ControllerBase
{
  private readonly ITourGuideService _tourGuideService;
  private readonly IAdminTourGuideService _adminTourGuideService;

  /// <summary>
  /// Initializes a new instance of the TourGuideController
  /// </summary>
  /// <param name="tourGuideService">The tour guide service</param>
  /// <param name="adminTourGuideService">The admin tour guide service</param>
  public TourGuideController(ITourGuideService tourGuideService, IAdminTourGuideService adminTourGuideService)
  {
    _tourGuideService = tourGuideService;
    _adminTourGuideService = adminTourGuideService;
  }

  /// <summary>
  /// Gets all tour guides based on the provided query parameters
  /// </summary>
  /// <param name="query">Query parameters for filtering tour guides</param>
  /// <returns>A list of tour guides matching the query criteria</returns>
  /// <response code="200">Returns the list of tour guides</response>
  [HttpGet]
  public async Task<IActionResult> GetAll([FromQuery] TourGuideQuery query)
  {
    return await _tourGuideService.GetTourGuideByQueryAsync(query);
  }

  /// <summary>
  /// Gets a specific tour guide by ID
  /// </summary>
  /// <param name="id">The ID of the tour guide</param>
  /// <returns>The requested tour guide</returns>
  /// <response code="200">Returns the requested tour guide</response>
  /// <response code="404">If the tour guide is not found</response>
  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(Guid id)
  {
    return await _tourGuideService.GetTourGuideByIdAsync(id);
  }

  /// <summary>
  /// Creates a new tour guide for a specific museum
  /// </summary>
  /// <param name="museumId">The ID of the museum</param>
  /// <param name="tourGuideCreateDto">The tour guide creation data</param>
  /// <returns>The newly created tour guide</returns>
  /// <response code="201">Returns the newly created tour guide</response>
  /// <response code="400">If the request data is invalid</response>
  [Protected]
  [HttpPost("/api/v1/museums/{museumId}/tour-guides")]
  public async Task<IActionResult> Create(Guid museumId, TourGuideCreateDto tourGuideCreateDto)
  {
    return await _adminTourGuideService.CreateTourGuideAsync(tourGuideCreateDto, museumId);
  }

  /// <summary>
  /// Updates an existing tour guide
  /// </summary>
  /// <param name="id">The ID of the tour guide to update</param>
  /// <param name="tourGuideUpdateDto">The updated tour guide data</param>
  /// <returns>The updated tour guide</returns>
  /// <response code="200">Returns the updated tour guide</response>
  /// <response code="404">If the tour guide is not found</response>
  [Protected]
  [HttpPut("{id}")]
  public async Task<IActionResult> Update(Guid id, TourGuideUpdateDto tourGuideUpdateDto)
  {
    return await _adminTourGuideService.UpdateTourGuideAsync(id, tourGuideUpdateDto);
  }

  /// <summary>
  /// Deletes a tour guide
  /// </summary>
  /// <param name="id">The ID of the tour guide to delete</param>
  /// <returns>No content</returns>
  /// <response code="204">If the tour guide was successfully deleted</response>
  /// <response code="404">If the tour guide is not found</response>
  [Protected]
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(Guid id)
  {
    return await _adminTourGuideService.DeleteTourGuideAsync(id);
  }

  /// <summary>
  /// Gets all tour guides associated with a specific event
  /// </summary>
  /// <param name="eventId">The ID of the event</param>
  /// <returns>List of tour guides for the specified event</returns>
  /// <response code="200">Returns the list of tour guides</response>
  [HttpGet("/api/v1/events/{eventId}/tour-guides")]
  public async Task<IActionResult> GetByEventId(Guid eventId)
  {
    return await _tourGuideService.GetTourGuideByEventIdAsync(eventId);
  }

  /// <summary>
  /// Gets all tour guides associated with a specific museum
  /// </summary>
  /// <param name="museumId">The ID of the museum</param>
  /// <returns>List of tour guides for the specified museum</returns>
  /// <response code="200">Returns the list of tour guides</response>
  [HttpGet("/api/v1/museums/{museumId}/tour-guides")]
  public async Task<IActionResult> GetByMuseumId(Guid museumId)
  {
    return await _tourGuideService.GetTourGuideByMuseumIdAsync(museumId);
  }

  /// <summary>
  /// Gets all tour guides associated with a specific user
  /// </summary>
  /// <param name="userId">The ID of the user</param>
  /// <returns>List of tour guides for the specified user</returns>
  /// <response code="200">Returns the list of tour guides</response>
  [HttpGet("/api/v1/users/{userId}/tour-guides")]
  public async Task<IActionResult> GetByUserId(Guid userId)
  {
    return await _tourGuideService.GetTourGuideByUserIdAsync(userId);
  }
}