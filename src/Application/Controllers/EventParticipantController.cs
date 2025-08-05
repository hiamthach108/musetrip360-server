using Microsoft.AspNetCore.Mvc;
using Application.Service;
using Application.Middlewares;

/// <summary>
/// Controller for managing event participants
/// </summary>
[Route("api/v1/event-participants")]
[ApiController]
[Produces("application/json")]
public class EventParticipantController : ControllerBase
{
    private readonly IEventParticipantService _eventParticipantService;
    private readonly ILogger<EventParticipantController> _logger;

    public EventParticipantController(IEventParticipantService eventParticipantService, ILogger<EventParticipantController> logger)
    {
        _eventParticipantService = eventParticipantService;
        _logger = logger;
    }

    /// <summary>
    /// Get all event participants
    /// </summary>
    /// <returns>A list of all event participants</returns>
    /// <response code="200">Returns the list of event participants</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    [Protected]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Get all event participants request received");
        return await _eventParticipantService.HandleGetAll();
    }

    /// <summary>
    /// Get an event participant by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the event participant</param>
    /// <returns>The event participant if found</returns>
    /// <response code="200">Returns the requested event participant</response>
    /// <response code="404">Event participant not found</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    [Protected]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Get event participant by ID request received: {Id}", id);
        return await _eventParticipantService.HandleGetById(id);
    }

    /// <summary>
    /// Get all participants for a specific event
    /// </summary>
    /// <param name="eventId">The unique identifier of the event</param>
    /// <returns>A list of participants for the event</returns>
    /// <response code="200">Returns the list of event participants</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    [Protected]
    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetByEventId(Guid eventId)
    {
        _logger.LogInformation("Get event participants by event ID request received: {EventId}", eventId);
        return await _eventParticipantService.HandleGetByEventId(eventId);
    }

    /// <summary>
    /// Get all events a user is participating in
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>A list of events the user is participating in</returns>
    /// <response code="200">Returns the list of events</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    [Protected]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        _logger.LogInformation("Get event participants by user ID request received: {UserId}", userId);
        return await _eventParticipantService.HandleGetByUserId(userId);
    }

    /// <summary>
    /// Check if a specific user is participating in a specific event
    /// </summary>
    /// <param name="eventId">The unique identifier of the event</param>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>The event participant if found</returns>
    /// <response code="200">Returns the event participant</response>
    /// <response code="404">Event participant not found</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    [Protected]
    [HttpGet("event/{eventId}/user/{userId}")]
    public async Task<IActionResult> GetByEventIdAndUserId(Guid eventId, Guid userId)
    {
        _logger.LogInformation("Get event participant by event ID and user ID request received: EventId={EventId}, UserId={UserId}", eventId, userId);
        return await _eventParticipantService.HandleGetByEventIdAndUserId(eventId, userId);
    }

    /// <summary>
    /// Add a new participant to an event
    /// </summary>
    /// <param name="eventParticipant">The event participant data</param>
    /// <returns>The created event participant</returns>
    /// <response code="201">Returns the newly created event participant</response>
    /// <response code="400">Invalid input data or user already participating</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Event or user not found</response>
    [Protected]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventParticipantCreateDto eventParticipant)
    {
        _logger.LogInformation("Create event participant request received: EventId={EventId}, UserId={UserId}", eventParticipant.EventId, eventParticipant.UserId);
        return await _eventParticipantService.HandleAdd(eventParticipant);
    }

    /// <summary>
    /// Update an existing event participant
    /// </summary>
    /// <param name="id">The unique identifier of the event participant</param>
    /// <param name="eventParticipant">The updated event participant data</param>
    /// <returns>The updated event participant</returns>
    /// <response code="200">Returns the updated event participant</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Event participant not found</response>
    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EventParticipantUpdateDto eventParticipant)
    {
        _logger.LogInformation("Update event participant request received: Id={Id}", id);
        return await _eventParticipantService.HandleUpdate(id, eventParticipant);
    }

    /// <summary>
    /// Remove a participant from an event
    /// </summary>
    /// <param name="id">The unique identifier of the event participant</param>
    /// <returns>Success message</returns>
    /// <response code="200">Event participant removed successfully</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Event participant not found</response>
    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Delete event participant request received: Id={Id}", id);
        return await _eventParticipantService.HandleDelete(id);
    }

    /// <summary>
    /// Add a new participant to an event
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="eventId">The unique identifier of the event</param>
    /// <returns>The created event participant</returns>    
    /// <response code="201">Returns the newly created event participant</response>
    /// <response code="400">Invalid input data or user already participating</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Event or user not found</response>
    [Protected]
    [HttpPost("client")]
    public async Task<IActionResult> AddClientEvent(Guid userId, Guid eventId)
    {
        _logger.LogInformation("Add client event participant request received: EventId={EventId}, UserId={UserId}", eventId, userId);
        return await _eventParticipantService.HandleAddClientEvent(userId, eventId);
    }
}