using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.RepresentationMaterial;

namespace MuseTrip360.src.Application.Controllers;

/// <summary>
/// Controller for managing representation materials for events
/// </summary>
[Route("api/v1/representation-materials")]
[ApiController]
[Produces("application/json")]
public class RepresentationMaterialController : ControllerBase
{
    private readonly IRepresentationMaterialService _representationMaterialService;

    public RepresentationMaterialController(IRepresentationMaterialService representationMaterialService)
    {
        _representationMaterialService = representationMaterialService;
    }

    /// <summary>
    /// Get all representation materials
    /// </summary>
    /// <returns>A list of all representation materials</returns>
    /// <response code="200">Returns the list of representation materials</response>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await _representationMaterialService.HandleGetAll();
    }

    /// <summary>
    /// Get a representation material by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the representation material</param>
    /// <returns>The representation material if found</returns>
    /// <response code="200">Returns the requested representation material</response>
    /// <response code="404">Representation material not found</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _representationMaterialService.HandleGetById(id);
    }

    /// <summary>
    /// Create a new representation material for an event
    /// </summary>
    /// <param name="eventId">The unique identifier of the event</param>
    /// <param name="dto">The representation material creation data</param>
    /// <returns>The created representation material</returns>
    /// <response code="201">Returns the newly created representation material</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Event not found</response>
    [Protected]
    [HttpPost("/api/v1/events/{eventId}/representation-materials")]
    public async Task<IActionResult> Create(Guid eventId, [FromBody] RepresentationMaterialCreateDto dto)
    {
        return await _representationMaterialService.HandleCreate(eventId, dto);
    }

    /// <summary>
    /// Update an existing representation material
    /// </summary>
    /// <param name="id">The unique identifier of the representation material to update</param>
    /// <param name="dto">The representation material update data</param>
    /// <returns>The updated representation material</returns>
    /// <response code="200">Returns the updated representation material</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Representation material not found</response>
    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RepresentationMaterialUpdateDto dto)
    {
        return await _representationMaterialService.HandleUpdate(id, dto);
    }

    /// <summary>
    /// Delete a representation material
    /// </summary>
    /// <param name="id">The unique identifier of the representation material to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">Representation material deleted successfully</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Representation material not found</response>
    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _representationMaterialService.HandleDelete(id);
    }
}