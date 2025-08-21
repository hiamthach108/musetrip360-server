using Microsoft.AspNetCore.Mvc;
using Application.Service;
using Application.Middlewares;
using Application.Shared.Enum;
using Core.Jwt;
using Application.Shared.Type;

/// <summary>
/// Controller for managing museum events, exhibitions, and special programs
/// </summary>
[Route("api/v1/events")]
[ApiController]
[Produces("application/json")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IAdminEventService _adminEventService;
    private readonly IOrganizerEventService _organizerEventService;

    public EventController(IEventService eventService, IAdminEventService adminEventService, IOrganizerEventService organizerEventService)
    {
        _eventService = eventService;
        _adminEventService = adminEventService;
        _organizerEventService = organizerEventService;
    }

    /// <summary>
    /// Get a paginated list of active events
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>A list of active events and total count</returns>
    /// <response code="200">Returns the list of events</response>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EventQuery query)
    {
        return await _eventService.HandleGetAll(query);
    }

    /// <summary>
    /// Get an event by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <returns>The event if found</returns>
    /// <response code="200">Returns the requested event</response>
    /// <response code="404">Event not found</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _eventService.HandleGetEventById(id);
    }

    /// <summary>
    /// Get a paginated list of all events (including inactive) for admin purposes
    /// </summary>
    /// <param name="query">Admin query parameters for filtering and pagination</param>
    /// <returns>A list of all events and total count</returns>
    /// <response code="200">Returns the list of events</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] EventAdminQuery query)
    {
        return await _adminEventService.HandleGetAllAdmin(query);
    }

    /// <summary>
    /// Get all events for a specific museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>A list of events belonging to the museum</returns>
    /// <response code="200">Returns the list of events</response>
    /// <response code="404">Museum not found</response>
    [HttpGet("/api/v1/museums/{museumId}/events")]
    public async Task<IActionResult> GetByMuseumId(Guid museumId, [FromQuery] EventAdminQuery query)
    {
        return await _eventService.HandleGetEventsByMuseumId(museumId, query);
    }

    /// <summary>
    /// Create a draft event request for a museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <param name="dto">The event creation data</param>
    /// <returns>The created draft event</returns>
    /// <response code="201">Returns the newly created draft event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have organizer privileges</response>
    /// <response code="404">Museum not found</response>
    [Protected]
    [HttpPost("/api/v1/museums/{museumId}/events/request")]
    public async Task<IActionResult> Create(Guid museumId, EventCreateDto dto)
    {
        return await _organizerEventService.HandleCreateDraft(museumId, dto);
    }

    /// <summary>
    /// Create a new event directly as an admin
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <param name="dto">The event creation data with admin privileges</param>
    /// <returns>The created event</returns>
    /// <response code="201">Returns the newly created event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Museum not found</response>
    [Protected]
    [HttpPost("/api/v1/museums/{museumId}/events")]
    public async Task<IActionResult> CreateAdmin(Guid museumId, EventCreateAdminDto dto)
    {
        return await _adminEventService.HandleCreateAdmin(museumId, dto);
    }

    /// <summary>
    /// Evaluate an event request (approve or reject)
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <param name="isApproved">Whether to approve or reject the event</param>
    /// <returns>The evaluated event</returns>
    /// <response code="200">Returns the evaluated event</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event not found</response>
    [Protected]
    [HttpPatch("{id}/evaluate")]
    public async Task<IActionResult> Evaluate(Guid id, bool isApproved)
    {
        return await _adminEventService.HandleEvaluateEvent(id, isApproved);
    }

    /// <summary>
    /// Cancel an event
    /// </summary>
    /// <param name="id">The unique identifier of the event to cancel</param>
    /// <returns>The cancelled event</returns>
    /// <response code="200">Returns the cancelled event</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event not found</response>
    [Protected]
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return await _adminEventService.HandleCancelEvent(id);
    }

    /// <summary>
    /// Update an event as an admin
    /// </summary>
    /// <param name="id">The unique identifier of the event to update</param>
    /// <param name="dto">The updated event data</param>
    /// <returns>The updated event</returns>
    /// <response code="200">Returns the updated event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event not found</response>
    [Protected]
    [HttpPut("{id}/admin")]
    public async Task<IActionResult> UpdateAdmin(Guid id, EventUpdateDto dto)
    {
        return await _adminEventService.HandleUpdateAdmin(id, dto);
    }

    /// <summary>
    /// Delete an event as an admin
    /// </summary>
    /// <param name="id">The unique identifier of the event to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Event successfully deleted</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event not found</response>
    [Protected]
    [HttpDelete("{id}/admin")]
    public async Task<IActionResult> DeleteAdmin(Guid id)
    {
        return await _adminEventService.HandleDeleteAdmin(id);
    }

    /// <summary>
    /// Submit an event for approval
    /// </summary>
    /// <param name="id">The unique identifier of the event to submit</param>
    /// <returns>The submitted event</returns>
    /// <response code="200">Returns the submitted event</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have organizer privileges</response>
    /// <response code="404">Event not found</response>
    [Protected]
    [HttpPatch("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        return await _organizerEventService.HandleSubmitEvent(id);
    }

    /// <summary>
    /// Get all events assigned to the current organizer
    /// </summary>
    /// <param name="status">Optional status filter for the events</param>
    /// <returns>A list of events assigned to the organizer</returns>
    /// <response code="200">Returns the list of events</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have organizer privileges</response>
    [Protected]
    [HttpGet("assigned")]
    public async Task<IActionResult> GetAllByOrganizer([FromQuery] EventStatusEnum? status)
    {
        return await _organizerEventService.HandleGetAllByOrganizer(status);
    }

    /// <summary>
    /// Update an event as an organizer
    /// </summary>
    /// <param name="id">The unique identifier of the event to update</param>
    /// <param name="dto">The updated event data</param>
    /// <returns>The updated event</returns>
    /// <response code="200">Returns the updated event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have organizer privileges</response>
    /// <response code="404">Event not found</response>
    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, EventUpdateDto dto)
    {
        return await _organizerEventService.HandleUpdate(id, dto);
    }

    /// <summary>
    /// Delete an event as an organizer
    /// </summary>
    /// <param name="id">The unique identifier of the event to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Event successfully deleted</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have organizer privileges</response>
    /// <response code="404">Event not found</response>
    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _organizerEventService.HandleDelete(id);
    }

    /// <summary>
    /// Add artifacts to an event (same museum)
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <param name="artifactIds">Collection of artifact IDs to add</param>
    /// <returns>The updated event</returns>
    /// <response code="200">Returns the updated event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event or artifacts not found</response>
    [Protected]
    [HttpPut("{id}/add-artifacts")]
    public async Task<IActionResult> AddArtifacts(Guid id, IEnumerable<Guid> artifactIds)
    {
        return await _adminEventService.HandleAddArtifactToEvent(id, artifactIds);
    }

    /// <summary>
    /// Remove artifacts from an event (same museum)
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <param name="artifactIds">Collection of artifact IDs to remove</param>
    /// <returns>The updated event</returns>
    /// <response code="200">Returns the updated event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event or artifacts not found</response>
    [Protected]
    [HttpPut("{id}/remove-artifacts")]
    public async Task<IActionResult> RemoveArtifacts(Guid id, IEnumerable<Guid> artifactIds)
    {
        return await _adminEventService.HandleRemoveArtifactFromEvent(id, artifactIds);
    }

    /// <summary>
    /// Add online tours to an event (same museum)
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <param name="tourOnlineIds">Collection of online tour IDs to add</param>
    /// <returns>The updated event</returns>
    /// <response code="200">Returns the updated event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event or online tours not found</response>
    [Protected]
    [HttpPut("{id}/add-tour-onlines")]
    public async Task<IActionResult> AddTourOnlines(Guid id, IEnumerable<Guid> tourOnlineIds)
    {
        return await _adminEventService.HandleAddTourOnlineToEvent(id, tourOnlineIds);
    }

    /// <summary>
    /// Remove online tours from an event (same museum)
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <param name="tourOnlineIds">Collection of online tour IDs to remove</param>
    /// <returns>The updated event</returns>
    /// <response code="200">Returns the updated event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event or online tours not found</response>
    [Protected]
    [HttpPut("{id}/remove-tour-onlines")]
    public async Task<IActionResult> RemoveTourOnlines(Guid id, IEnumerable<Guid> tourOnlineIds)
    {
        return await _adminEventService.HandleRemoveTourOnlineFromEvent(id, tourOnlineIds);
    }

    /// <summary>
    /// Add tour guides to an event (same museum)
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <param name="tourGuideIds">Collection of tour guide IDs to add</param>
    /// <returns>The updated event</returns>
    /// <response code="200">Returns the updated event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event or tour guides not found</response>
    [Protected]
    [HttpPut("{id}/add-tour-guides")]
    public async Task<IActionResult> AddTourGuides(Guid id, IEnumerable<Guid> tourGuideIds)
    {
        return await _adminEventService.HandleAddTourGuideToEvent(id, tourGuideIds);
    }

    /// <summary>
    /// Remove tour guides from an event (same museum)
    /// </summary>
    /// <param name="id">The unique identifier of the event</param>
    /// <param name="tourGuideIds">Collection of tour guide IDs to remove</param>
    /// <returns>The updated event</returns>
    /// <response code="200">Returns the updated event</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Event or tour guides not found</response>
    [Protected]
    [HttpPut("{id}/remove-tour-guides")]
    public async Task<IActionResult> RemoveTourGuides(Guid id, IEnumerable<Guid> tourGuideIds)
    {
        return await _adminEventService.HandleRemoveTourGuideFromEvent(id, tourGuideIds);
    }
    /// <summary>
    /// Get all events created by the current user
    /// </summary>
    /// <returns>A list of events created by the current user</returns>
    /// <response code="200">Returns the list of events</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    [Protected]
    [HttpGet("created-by/{userId}")]
    public async Task<IActionResult> GetEventCreatedByUser(Guid userId)
    {
        return await _adminEventService.HandleGetEventCreatedByUser(userId);
    }
}
