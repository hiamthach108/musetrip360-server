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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _eventService.HandleGetEventById(id);
    }

    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] EventAdminQuery query)
    {
        return await _eventService.HandleGetAllAdmin(query);
    }

    [HttpGet("/api/v1/museums/{museumId}/events")]
    public async Task<IActionResult> GetByMuseumId(Guid museumId)
    {
        return await _eventService.HandleGetEventsByMuseumId(museumId);
    }

    [Protected]
    [HttpPost("/api/v1/museums/{museumId}/events/request")]
    public async Task<IActionResult> Create(Guid museumId, EventCreateDto dto)
    {
        return await _eventService.HandleCreateByOrganizer(museumId, dto);
    }

    [Protected]
    [HttpPatch("{id}/evaluate")]
    public async Task<IActionResult> Evaluate(Guid id, bool isApproved)
    {
        return await _eventService.HandleEvaluateEvent(id, isApproved);
    }

    [Protected]
    [HttpPatch("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        return await _eventService.HandleSubmitEvent(id);
    }

    [Protected]
    [HttpGet("draft")]
    public async Task<IActionResult> GetDraft()
    {
        return await _eventService.HandleGetDraftEventByOrganizer();
    }

    [Protected]
    [HttpGet("submitted")]
    public async Task<IActionResult> GetSubmitted()
    {
        return await _eventService.HandleGetSubmmittedEventByOrganizer();
    }

    [Protected]
    [HttpGet("expired")]
    public async Task<IActionResult> GetExpired()
    {
        return await _eventService.HandleGetExpiredEventByOrganizer();
    }

    [Protected]
    [HttpGet("owner")]
    public async Task<IActionResult> GetAllByOrganizer()
    {
        return await _eventService.HandleGetAllEventByOrganizer();
    }

    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, EventUpdateDto dto)
    {
        return await _eventService.HandleUpdate(id, dto);
    }

    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _eventService.HandleDelete(id);
    }
}
