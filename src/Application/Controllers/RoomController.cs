using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for managing room operations in the MuseTrip360 application.
/// Provides endpoints for creating, reading, updating, and deleting rooms.
/// </summary>
[Route("api/v1/rooms")]
[ApiController]
[Produces("application/json")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomController"/> class.
    /// </summary>
    /// <param name="roomService">The room service for handling business logic.</param>
    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Creates a new room.
    /// </summary>
    /// <param name="dto">The room creation data transfer object containing room details.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>201 Created - Room created successfully</description></item>
    /// <item><description>400 Bad Request - Invalid input data</description></item>
    /// <item><description>404 Not Found - Associated event not found</description></item>
    /// <item><description>500 Internal Server Error - Server error occurred</description></item>
    /// </list>
    /// </returns>
    /// <response code="201">Returns the created room information</response>
    /// <response code="400">If the dto is null or invalid</response>
    /// <response code="404">If the associated event does not exist</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("/api/v1/events/{eventId}/rooms")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(Guid eventId, RoomCreateDto dto)
    {
        return await _roomService.HandleCreate(eventId, dto);
    }

    /// <summary>
    /// Retrieves a room by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the room to retrieve.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>200 OK - Room found and returned</description></item>
    /// <item><description>404 Not Found - Room not found</description></item>
    /// <item><description>500 Internal Server Error - Server error occurred</description></item>
    /// </list>
    /// </returns>
    /// <response code="200">Returns the room information</response>
    /// <response code="404">If the room with the specified id does not exist</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(string id)
    {
        return await _roomService.HandleGetById(id);
    }

    /// <summary>
    /// Updates an existing room with new information.
    /// </summary>
    /// <param name="id">The unique identifier of the room to update.</param>
    /// <param name="dto">The room update data transfer object containing updated room details.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>200 OK - Room updated successfully</description></item>
    /// <item><description>400 Bad Request - Invalid input data</description></item>
    /// <item><description>404 Not Found - Room not found</description></item>
    /// <item><description>500 Internal Server Error - Server error occurred</description></item>
    /// </list>
    /// </returns>
    /// <response code="200">Returns success message</response>
    /// <response code="400">If the dto is null or invalid</response>
    /// <response code="404">If the room with the specified id does not exist</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(string id, RoomUpdateDto dto)
    {
        return await _roomService.HandleUpdate(id, dto);
    }

    /// <summary>
    /// Deletes a room by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the room to delete.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>200 OK - Room deleted successfully</description></item>
    /// <item><description>404 Not Found - Room not found</description></item>
    /// <item><description>500 Internal Server Error - Server error occurred</description></item>
    /// </list>
    /// </returns>
    /// <response code="200">Returns success message</response>
    /// <response code="404">If the room with the specified id does not exist</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string id)
    {
        return await _roomService.HandleDelete(id);
    }

    /// <summary>
    /// Updates the metadata of an existing room.
    /// </summary>
    /// <param name="id">The unique identifier of the room to update metadata for.</param>
    /// <param name="dto">The room metadata update data transfer object containing updated metadata.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>200 OK - Room metadata updated successfully</description></item>
    /// <item><description>400 Bad Request - Invalid input data</description></item>
    /// <item><description>404 Not Found - Room not found</description></item>
    /// <item><description>500 Internal Server Error - Server error occurred</description></item>
    /// </list>
    /// </returns>
    /// <response code="200">Returns success message</response>
    /// <response code="400">If the dto is null or invalid</response>
    /// <response code="404">If the room with the specified id does not exist</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}/metadata")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMetadata(string id, RoomUpdateMetadataDto dto)
    {
        return await _roomService.HandleUpdateMetadata(id, dto);
    }

    /// <summary>
    /// Retrieves a room by its event id.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>200 OK - Room found and returned</description></item>
    /// <item><description>404 Not Found - Room not found</description></item>
    /// <item><description>500 Internal Server Error - Server error occurred</description></item>
    /// </list>
    /// </returns>
    [HttpGet("/api/v1/events/{eventId}/rooms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRoomByEventId(Guid eventId)
    {
        return await _roomService.HandleGetRoomByEventId(eventId);
    }
}