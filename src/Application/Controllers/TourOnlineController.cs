using Application.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Domain.Tours;

/// <summary>
/// Controller for managing online tours and virtual museum experiences
/// </summary>
[Route("api/v1/tour-onlines")]
[ApiController]
[Produces("application/json")]
public class TourOnlineController : ControllerBase
{
    private readonly ITourOnlineService _tourOnlineService;
    private readonly IAdminTourOnlineService _adminTourOnlineService;

    public TourOnlineController(ITourOnlineService tourOnlineService, IAdminTourOnlineService adminTourOnlineService)
    {
        _tourOnlineService = tourOnlineService;
        _adminTourOnlineService = adminTourOnlineService;
    }

    /// <summary>
    /// Get a paginated list of active online tours
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>A list of active online tours and total count</returns>
    /// <response code="200">Returns the list of online tours</response>
    /// <response code="400">Invalid query parameters</response>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TourOnlineQuery query)
    {
        return await _tourOnlineService.GetAllAsync(query);
    }

    /// <summary>
    /// Get an online tour by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the online tour</param>
    /// <returns>The online tour if found</returns>
    /// <response code="200">Returns the requested online tour</response>
    /// <response code="404">Online tour not found</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        return await _tourOnlineService.GetByIdAsync(id);
    }

    /// <summary>
    /// Get all online tours for a specific museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>A list of online tours for the specified museum</returns>
    /// <response code="200">Returns the list of online tours</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Museum not found</response>
    [Protected]
    [HttpGet("/api/v1/museums/{museumId}/tour-onlines")]
    public async Task<IActionResult> GetByMuseumId([FromRoute] Guid museumId, [FromQuery] TourOnlineAdminQuery query)
    {
        return await _adminTourOnlineService.GetAllByMuseumIdAsync(museumId, query);
    }

    /// <summary>
    /// Create a new online tour for a museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum to create the tour for</param>
    /// <param name="tourOnline">The online tour creation data</param>
    /// <returns>The created online tour</returns>
    /// <response code="201">Returns the newly created online tour</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Museum not found</response>
    [Protected]
    [HttpPost("/api/v1/museums/{museumId}/tour-onlines")]
    public async Task<IActionResult> Create([FromRoute] Guid museumId, [FromBody] TourOnlineCreateDto tourOnline)
    {
        return await _adminTourOnlineService.CreateAsync(museumId, tourOnline);
    }

    /// <summary>
    /// Update an existing online tour
    /// </summary>
    /// <param name="id">The unique identifier of the online tour to update</param>
    /// <param name="tourOnline">The updated online tour data</param>
    /// <returns>The updated online tour</returns>
    /// <response code="200">Returns the updated online tour</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Online tour not found</response>
    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] TourOnlineUpdateDto tourOnline)
    {
        return await _adminTourOnlineService.UpdateAsync(id, tourOnline);
    }

    /// <summary>
    /// Delete an online tour
    /// </summary>
    /// <param name="id">The unique identifier of the online tour to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Online tour successfully deleted</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Online tour not found</response>
    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return await _adminTourOnlineService.DeleteAsync(id);
    }

    /// <summary>
    /// Activate an online tour
    /// </summary>
    /// <param name="id">The unique identifier of the online tour to activate</param>
    /// <returns>The activated online tour</returns>
    /// <response code="200">Returns the activated online tour</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Online tour not found</response>
    [Protected]
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate([FromRoute] Guid id)
    {
        return await _adminTourOnlineService.ActivateAsync(id);
    }

    /// <summary>
    /// Deactivate an online tour
    /// </summary>
    /// <param name="id">The unique identifier of the online tour to deactivate</param>
    /// <returns>The deactivated online tour</returns>
    /// <response code="200">Returns the deactivated online tour</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    /// <response code="404">Online tour not found</response>
    [Protected]
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid id)
    {
        return await _adminTourOnlineService.DeactivateAsync(id);
    }

    /// <summary>
    /// Get a paginated list of all online tours (including inactive) for admin purposes
    /// </summary>
    /// <param name="query">Admin query parameters for filtering and pagination</param>
    /// <returns>A list of all online tours and total count</returns>
    /// <response code="200">Returns the list of online tours</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] TourOnlineAdminQuery query)
    {
        return await _adminTourOnlineService.GetAllAdminAsync(query);
    }

    /// <summary>
    /// Add tour contents to an online tour
    /// </summary>
    /// <param name="id">The unique identifier of the online tour</param>
    /// <param name="tourContentIds">The unique identifiers of the tour contents to add</param>
    /// <returns>The updated online tour</returns>
    [Protected]
    [HttpPut("{id}/add-tour-contents")]
    public async Task<IActionResult> AddTourContents([FromRoute] Guid id, [FromBody] IEnumerable<Guid> tourContentIds)
    {
        return await _adminTourOnlineService.AddTourContentToTourAsync(id, tourContentIds);
    }

    /// <summary>
    /// Remove tour contents from an online tour
    /// </summary>
    /// <param name="id">The unique identifier of the online tour</param>
    /// <param name="tourContentIds">The unique identifiers of the tour contents to add</param>
    /// <returns>The updated online tour</returns>
    [Protected]
    [HttpPut("{id}/remove-tour-contents")]
    public async Task<IActionResult> RemoveTourContents([FromRoute] Guid id, [FromBody] IEnumerable<Guid> tourContentIds)
    {
        return await _adminTourOnlineService.RemoveTourContentFromTourAsync(id, tourContentIds);
    }

    /// <summary>
    /// Feedback an online tour
    /// </summary>
    /// <param name="id">The unique identifier of the online tour</param>
    /// <param name="dto">The comment of the rating</param>
    /// <returns>The updated online tour</returns>
    /// <response code="200">Returns the updated online tour</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Online tour not found</response>
    [Protected]
    [HttpPatch("{id}/feedback")]
    public async Task<IActionResult> Feedback([FromRoute] Guid id, [FromBody] FeedbackCreateDto dto)
    {
        return await _tourOnlineService.HandleFeedback(id, dto.Comment);
    }
}