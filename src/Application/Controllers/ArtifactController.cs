using Application.Middlewares;
using Application.Shared.Constant;
using Core.Jwt;
using Microsoft.AspNetCore.Mvc;
using MuseTrip360.src.Application.DTOs.Artifact;
using MuseTrip360.src.Application.DTOs.Feedback;
using MuseTrip360.src.Application.Service;

namespace MuseTrip360.src.Application.Controllers;

/// <summary>
/// Controller for managing museum artifacts and their virtual representations
/// </summary>
[Route("api/v1/artifacts")]
[ApiController]
[Produces("application/json")]
public class ArtifactController : ControllerBase
{
    private readonly IArtifactService _artifactService;

    public ArtifactController(IArtifactService artifactService)
    {
        _artifactService = artifactService;
    }

    /// <summary>
    /// Get a paginated list of active artifacts
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>A list of active artifacts and total count</returns>
    /// <response code="200">Returns the list of artifacts</response>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ArtifactQuery query)
    {
        return await _artifactService.HandleGetAll(query);
    }

    /// <summary>
    /// Get a paginated list of all artifacts (including inactive) for admin purposes
    /// </summary>
    /// <param name="query">Admin query parameters for filtering and pagination</param>
    /// <returns>A list of all artifacts and total count</returns>
    /// <response code="200">Returns the list of artifacts</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have admin privileges</response>
    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] ArtifactAdminQuery query)
    {
        return await _artifactService.HandleGetAllAdmin(query);
    }

    /// <summary>
    /// Get an artifact by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the artifact</param>
    /// <returns>The artifact if found</returns>
    /// <response code="200">Returns the requested artifact</response>
    /// <response code="404">Artifact not found</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _artifactService.HandleGetById(id);
    }

    /// <summary>
    /// Get all artifacts for a specific museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>A list of artifacts belonging to the museum</returns>
    /// <response code="200">Returns the list of artifacts</response>
    /// <response code="404">Museum not found</response>
    [HttpGet("/api/v1/museums/{museumId}/artifacts")]
    public async Task<IActionResult> GetByMuseumId(Guid museumId, [FromQuery] ArtifactAdminQuery query)
    {
        return await _artifactService.HandleGetByMuseumId(museumId, query);
    }

    /// <summary>
    /// Create a new artifact for a museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <param name="dto">The artifact creation data</param>
    /// <returns>The created artifact</returns>
    /// <response code="201">Returns the newly created artifact</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Museum not found</response>
    [Protected]
    [HttpPost("/api/v1/museums/{museumId}/artifacts")]
    public async Task<IActionResult> Create(Guid museumId, ArtifactCreateDto dto)
    {
        return await _artifactService.HandleCreate(museumId, dto);
    }

    /// <summary>
    /// Update an existing artifact
    /// </summary>
    /// <param name="id">The unique identifier of the artifact to update</param>
    /// <param name="dto">The updated artifact data</param>
    /// <returns>The updated artifact</returns>
    /// <response code="200">Returns the updated artifact</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Artifact not found</response>
    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, ArtifactUpdateDto dto)
    {
        return await _artifactService.HandleUpdate(id, dto);
    }

    /// <summary>
    /// Delete an artifact
    /// </summary>
    /// <param name="id">The unique identifier of the artifact to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Artifact successfully deleted</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Artifact not found</response>
    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _artifactService.HandleDelete(id);
    }

    /// <summary>
    /// Activate an artifact
    /// </summary>
    /// <param name="id">The unique identifier of the artifact to activate</param>
    /// <returns>The activated artifact</returns>
    /// <response code="200">Returns the activated artifact</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Artifact not found</response>
    [Protected]
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        return await _artifactService.HandleActivate(id);
    }

    /// <summary>
    /// Deactivate an artifact
    /// </summary>
    /// <param name="id">The unique identifier of the artifact to deactivate</param>
    /// <returns>The deactivated artifact</returns>
    /// <response code="200">Returns the deactivated artifact</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User does not have required privileges</response>
    /// <response code="404">Artifact not found</response>
    [Protected]
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return await _artifactService.HandleDeactivate(id);
    }

    /// <summary>
    /// Get artifacts by filter and sort
    /// </summary>
    /// <param name="filterSort">Filter and sort parameters</param>
    /// <returns>A list of artifacts matching the filter and sort criteria</returns>
    /// <response code="200">Returns the list of artifacts</response>
    /// <response code="400">Invalid filter or sort parameters</response>
    [HttpGet("filter-sort")]
    public async Task<IActionResult> GetByFilterSort([FromQuery] ArtifactFilterSort filterSort)
    {
        return await _artifactService.HandleGetByFilterSort(filterSort);
    }
}