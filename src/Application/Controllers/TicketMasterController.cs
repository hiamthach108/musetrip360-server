using Application.Service;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for managing ticket master operations
/// </summary>
[Route("api/v1/ticket-masters")]
[ApiController]
public class TicketMasterController(ITicketMasterService ticketMasterService) : ControllerBase
{
    /// <summary>
    /// Get a list of ticket masters based on query parameters
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>List of ticket masters matching the query criteria</returns>
    /// <response code="200">Returns the list of ticket masters</response>
    /// <response code="400">If the query parameters are invalid</response>
    [HttpGet]
    public async Task<IActionResult> GetTicketMasterQuery([FromQuery] TicketMasterQuery query)
    {
        return await ticketMasterService.GetTicketMasterQuery(query);
    }

    /// <summary>
    /// Get a specific ticket master by ID
    /// </summary>
    /// <param name="id">The unique identifier of the ticket master</param>
    /// <returns>The ticket master details</returns>
    /// <response code="200">Returns the requested ticket master</response>
    /// <response code="404">If the ticket master is not found</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketMasterById(Guid id)
    {
        return await ticketMasterService.GetTicketMasterById(id);
    }

    /// <summary>
    /// Get all ticket masters for a specific museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <returns>List of ticket masters associated with the museum</returns>
    /// <response code="200">Returns the list of ticket masters for the museum</response>
    /// <response code="404">If the museum is not found</response>
    [HttpGet("/api/v1/museums/{museumId}/ticket-masters")]
    public async Task<IActionResult> GetTicketMasterByMuseumId(Guid museumId)
    {
        return await ticketMasterService.GetTicketMasterByMuseumId(museumId);
    }

    /// <summary>
    /// Create a new ticket master for a museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <param name="dto">The ticket master creation data</param>
    /// <returns>The created ticket master details</returns>
    /// <response code="201">Returns the newly created ticket master</response>
    /// <response code="400">If the input data is invalid</response>
    /// <response code="404">If the museum is not found</response>
    [HttpPost("/api/v1/museums/{museumId}/ticket-masters")]
    public async Task<IActionResult> CreateTicketMaster(Guid museumId, [FromBody] TicketMasterCreateDto dto)
    {
        return await ticketMasterService.CreateTicketMaster(museumId, dto);
    }

    /// <summary>
    /// Update an existing ticket master
    /// </summary>
    /// <param name="id">The unique identifier of the ticket master to update</param>
    /// <param name="dto">The updated ticket master data</param>
    /// <returns>The updated ticket master details</returns>
    /// <response code="200">Returns the updated ticket master</response>
    /// <response code="400">If the input data is invalid</response>
    /// <response code="404">If the ticket master is not found</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicketMaster(Guid id, [FromBody] TicketMasterUpdateDto dto)
    {
        return await ticketMasterService.UpdateTicketMaster(id, dto);
    }

    /// <summary>
    /// Delete a ticket master
    /// </summary>
    /// <param name="id">The unique identifier of the ticket master to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">If the ticket master was successfully deleted</response>
    /// <response code="404">If the ticket master is not found</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicketMaster(Guid id)
    {
        return await ticketMasterService.DeleteTicketMaster(id);
    }
}