using Application.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Domain.Tours;

/// <summary>
/// Controller for managing online tours and virtual museum experiences
/// </summary>
[Route("api/tour-online")]
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
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        return await _tourOnlineService.GetByIdAsync(id);
    }

    /// <summary>
    /// Get all online tours for a specific museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <returns>A list of online tours for the specified museum</returns>
    /// <response code="200">Returns the list of online tours</response>
    [Protected]
    [HttpGet("museum/{museumId}")]

    public async Task<IActionResult> GetByMuseumId([FromRoute] Guid museumId)
    {
        return await _adminTourOnlineService.GetAllByMuseumIdAsync(museumId);
    }

    /// <summary>
    /// Create a new online tour
    /// </summary>
    /// <param name="tourOnline">The online tour creation data</param>
    /// <returns>The created online tour</returns>
    /// <response code="201">Returns the newly created online tour</response>
    [Protected]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TourOnlineCreateDto tourOnline)
    {
        return await _adminTourOnlineService.CreateAsync(tourOnline);
    }

    /// <summary>
    /// Update an existing online tour
    /// </summary>
    /// <param name="id">The unique identifier of the online tour to update</param>
    /// <param name="tourOnline">The updated online tour data</param>
    /// <returns>The updated online tour</returns>
    /// <response code="200">Returns the updated online tour</response>
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
    [Protected]
    [HttpPut("activate/{id}")]
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
    [Protected]
    [HttpPut("deactivate/{id}")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid id)
    {
        return await _adminTourOnlineService.DeactivateAsync(id);
    }

    /// <summary>
    /// Get a paginated list of all online tours (including inactive) for admin purposes
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>A list of all online tours and total count</returns>
    /// <response code="200">Returns the list of online tours</response>
    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] TourOnlineAdminQuery query)
    {
        return await _adminTourOnlineService.GetAllAdminAsync(query);
    }
}
