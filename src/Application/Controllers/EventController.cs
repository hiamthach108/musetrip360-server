using Microsoft.AspNetCore.Mvc;
using Application.Service;
using Application.Middlewares;
[Route("api/v1/events")]
[ApiController]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;   

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EventQuery query)
    {
        return await _eventService.HandleGetAll(query);
    }
    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] EventAdminQuery query)
    {
        return await _eventService.HandleGetAllAdmin(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _eventService.HandleGetEventById(id);
    }

    [HttpGet("/api/v1/museums/{museumId}/events")]
    public async Task<IActionResult> GetByMuseumId(Guid museumId)
    {
        return await _eventService.HandleGetEventsByMuseumId(museumId);
    }

    [HttpPost("/api/v1/museums/{museumId}/events")]
    public async Task<IActionResult> Create(Guid museumId, EventCreateDto dto)
    {
        return await _eventService.HandleCreate(museumId, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, EventUpdateDto dto)
    {
        return await _eventService.HandleUpdate(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _eventService.HandleDelete(id);
    }
}
