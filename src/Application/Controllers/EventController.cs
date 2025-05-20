using Microsoft.AspNetCore.Mvc;
using Application.Service;
using Application.Middlewares;
using Application.Shared.Enum;
using Core.Jwt;
using Application.Shared.Type;
[Route("api/v1/events")]
[ApiController]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IAdminEventService _adminEventService;
    private readonly IOrganizerEventService _organizerEventService;

    public EventController(IEventService eventService, IAdminEventService adminEventService, IOrganizerEventService organizerEventService)
    {
        _eventService = eventService;
        _adminEventService = adminEventService;
        _organizerEventService = organizerEventService;
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
        return await _adminEventService.HandleGetAllAdmin(query);
    }

    [HttpGet("museums/{museumId}")]
    public async Task<IActionResult> GetByMuseumId(Guid museumId)
    {
        return await _eventService.HandleGetEventsByMuseumId(museumId);
    }

    [Protected]
    [HttpPost("museums/{museumId}/request")]
    public async Task<IActionResult> Create(Guid museumId, EventCreateDto dto)
    {
        var payload = HttpContext.Items["payload"] as Payload;
        return await _organizerEventService.HandleCreateDraft(payload!.UserId, museumId, dto);
    }

    [Protected]
    [HttpPost("admin/museums/{museumId}")]
    public async Task<IActionResult> CreateAdmin(Guid museumId, EventCreateAdminDto dto)
    {
        var payload = HttpContext.Items["payload"] as Payload;
        //check admin
        return await _adminEventService.HandleCreateAdmin(payload!.UserId, museumId, dto);
    }

    [Protected]
    [HttpPatch("{id}/evaluate")]
    public async Task<IActionResult> Evaluate(Guid id, bool isApproved)
    {
        return await _adminEventService.HandleEvaluateEvent(id, isApproved);
    }

    [Protected]
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return await _adminEventService.HandleCancelEvent(id);
    }

    [Protected]
    [HttpPut("{id}/admin")]
    public async Task<IActionResult> UpdateAdmin(Guid id, EventUpdateDto dto)
    {
        return await _adminEventService.HandleUpdateAdmin(id, dto);
    }

    [Protected]
    [HttpDelete("{id}/admin")]
    public async Task<IActionResult> DeleteAdmin(Guid id)
    {
        return await _adminEventService.HandleDeleteAdmin(id);
    }

    [Protected]
    [HttpPatch("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        return await _organizerEventService.HandleSubmitEvent(id);
    }

    [Protected]
    [HttpGet("assigned")]
    public async Task<IActionResult> GetAllByOrganizer([FromQuery] EventStatusEnum? status)
    {
        var payload = HttpContext.Items["payload"] as Payload;
        return await _organizerEventService.HandleGetAllByOrganizer(payload!.UserId, status);
    }

    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, EventUpdateDto dto)
    {
        return await _organizerEventService.HandleUpdate(id, dto);
    }

    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _organizerEventService.HandleDelete(id);
    }
    [Protected]
    [HttpPut("{id}/add-artifacts")]
    public async Task<IActionResult> AddArtifacts(Guid id, IEnumerable<Guid> artifactIds)
    {
        return await _adminEventService.HandleAddArtifactToEvent(id, artifactIds);
    }
    [Protected]
    [HttpPut("{id}/remove-artifacts")]
    public async Task<IActionResult> RemoveArtifacts(Guid id, IEnumerable<Guid> artifactIds)
    {
        return await _adminEventService.HandleRemoveArtifactFromEvent(id, artifactIds);
    }
    [Protected]
    [HttpPut("{id}/add-tour-onlines")]
    public async Task<IActionResult> AddTourOnlines(Guid id, IEnumerable<Guid> tourOnlineIds)
    {
        return await _adminEventService.HandleAddTourOnlineToEvent(id, tourOnlineIds);
    }
    [Protected]
    [HttpPut("{id}/remove-tour-onlines")]
    public async Task<IActionResult> RemoveTourOnlines(Guid id, IEnumerable<Guid> tourOnlineIds)
    {
        return await _adminEventService.HandleRemoveTourOnlineFromEvent(id, tourOnlineIds);
    }
}
