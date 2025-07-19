using Application.Middlewares;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for managing tour content and virtual museum tour experiences
/// </summary>
[Route("api/v1/tour-contents")]
[ApiController]
[Produces("application/json")]
public class TourContentController : ControllerBase
{
    private readonly ITourContentService _tourContentService;
    private readonly IAdminTourContentService _adminTourContentService;

    public TourContentController(ITourContentService tourContentService, IAdminTourContentService adminTourContentService)
    {
        _tourContentService = tourContentService;
        _adminTourContentService = adminTourContentService;
    }

    /// <summary>
    /// Get a paginated list of active tour contents
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>A list of active tour contents and total count</returns>
    /// <response code="200">Returns the list of tour contents</response>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TourContentQuery query)
    {
        return await _tourContentService.HandleGetAllAsync(query);
    }

    /// <summary>
    /// Get a paginated list of all tour contents (including inactive) for admin purposes
    /// </summary>
    /// <param name="query">Admin query parameters for filtering and pagination</param>
    /// <returns>A list of all tour contents and total count</returns>
    /// <response code="200">Returns the list of tour contents</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] TourContentAdminQuery query)
    {
        return await _adminTourContentService.HandleGetAllAdminAsync(query);
    }

    /// <summary>
    /// Get a tour content by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the tour content</param>
    /// <returns>The tour content if found</returns>
    /// <response code="200">Returns the requested tour content</response>
    /// <response code="404">Tour content not found</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _tourContentService.HandleGetByIdAsync(id);
    }

    /// <summary>
    /// Create a new tour content
    /// </summary>
    /// <param name="tourOnlineId">The unique identifier of the tour online</param>
    /// <param name="dto">The tour content creation data</param>
    /// <returns>The created tour content</returns>
    /// <response code="201">Returns the newly created tour content</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    [Protected]
    [HttpPost("/api/v1/tour-onlines/{tourOnlineId}/contents")]
    public async Task<IActionResult> Create([FromRoute] Guid tourOnlineId, [FromBody] TourContentCreateDto dto)
    {
        return await _adminTourContentService.HandleCreateAsync(tourOnlineId, dto);
    }

    /// <summary>
    /// Update an existing tour content
    /// </summary>
    /// <param name="id">The unique identifier of the tour content to update</param>
    /// <param name="dto">The updated tour content data</param>
    /// <returns>The updated tour content</returns>
    /// <response code="200">Returns the updated tour content</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Tour content not found</response>
    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TourContentUpdateDto dto)
    {
        return await _adminTourContentService.HandleUpdateAsync(id, dto);
    }

    /// <summary>
    /// Delete a tour content
    /// </summary>
    /// <param name="id">The unique identifier of the tour content to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Tour content successfully deleted</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Tour content not found</response>
    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _adminTourContentService.HandleDeleteAsync(id);
    }

    /// <summary>
    /// Activate a tour content
    /// </summary>
    /// <param name="id">The unique identifier of the tour content to activate</param>
    /// <returns>The activated tour content</returns>
    /// <response code="200">Returns the activated tour content</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Tour content not found</response>
    [Protected]
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        return await _adminTourContentService.HandleActivateAsync(id);
    }

    /// <summary>
    /// Deactivate a tour content
    /// </summary>
    /// <param name="id">The unique identifier of the tour content to deactivate</param>
    /// <returns>The deactivated tour content</returns>
    /// <response code="200">Returns the deactivated tour content</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Tour content not found</response>
    [Protected]
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return await _adminTourContentService.HandleDeactivateAsync(id);
    }

    [Protected]
    [HttpGet("/api/v1/tour-onlines/{tourOnlineId}/contents")]
    public async Task<IActionResult> GetByTourOnlineId(Guid tourOnlineId, [FromQuery] TourContentAdminQuery query)
    {
        return await _adminTourContentService.HandleGetByTourOnlineIdAsync(tourOnlineId, query);
    }
}

