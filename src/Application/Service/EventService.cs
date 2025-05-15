namespace Application.Service;

using Application.Service;
using Application.Shared.Enum;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Events;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using MuseTrip360.src.Infrastructure.Repository;

// Normal user operations
public interface IEventService
{
    Task<IActionResult> HandleGetEventById(Guid id);
    Task<IActionResult> HandleGetAll(EventQuery query);
    Task<IActionResult> HandleGetEventsByMuseumId(Guid museumId);
}

// Admin, Manager operations
public interface IAdminEventService : IEventService
{
    Task<IActionResult> HandleGetAllAdmin(EventAdminQuery query);
    Task<IActionResult> HandleAddArtifactToEventSameMuseum(Guid eventId, List<Guid> artifactIds);
    Task<IActionResult> HandleEvaluateEvent(Guid id, bool isApproved);
    Task<IActionResult> HandleUpdateAdmin(Guid id, EventUpdateDto dto);
    Task<IActionResult> HandleDeleteAdmin(Guid id);
    Task<IActionResult> HandleCreateAdmin(Guid museumId, EventCreateAdminDto dto);
}

// Organizer, operations
public interface IOrganizerEventService : IEventService
{
    Task<IActionResult> HandleCreateDraft(Guid museumId, EventCreateDto dto);
    Task<IActionResult> HandleSubmitEvent(Guid id);
    Task<IActionResult> HandleGetDraft();
    Task<IActionResult> HandleGetSubmitted();
    Task<IActionResult> HandleGetExpired();
    Task<IActionResult> HandleGetAllByOrganizer();
    Task<IActionResult> HandleUpdate(Guid id, EventUpdateDto dto);
    Task<IActionResult> HandleDelete(Guid id);
}

// Base implementation for common operations
public abstract class BaseEventService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService(context, mapper, httpContextAccessor), IEventService
{
    protected readonly IEventRepository _eventRepository = new EventRepository(context);
    protected readonly IMuseumRepository _museumRepository = new MuseumRepository(context);
    protected readonly IArtifactRepository _artifactRepository = new ArtifactRepository(context);
    protected readonly IUserRepository _userRepository = new UserRepository(context);

    public virtual async Task<IActionResult> HandleGetEventById(Guid id)
    {
        try
        {
            var eventItem = await _eventRepository.GetEventById(id);
            var eventDto = _mapper.Map<EventDto>(eventItem);
            return SuccessResp.Ok(eventDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public virtual async Task<IActionResult> HandleGetAll(EventQuery query)
    {
        try
        {
            var events = await _eventRepository.GetAllAsync(query);
            var eventDtos = _mapper.Map<List<EventDto>>(events.Events);
            return SuccessResp.Ok(new { List = eventDtos, Total = events.Total });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public virtual async Task<IActionResult> HandleGetEventsByMuseumId(Guid museumId)
    {
        try
        {
            var events = await _eventRepository.GetEventsByMuseumIdAsync(museumId);
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}

public class EventService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseEventService(context, mapper, httpContextAccessor), IEventService
{
}

// Admin implementation
public class AdminEventService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseEventService(context, mapper, httpContextAccessor), IAdminEventService
{
    public async Task<IActionResult> HandleGetAllAdmin(EventAdminQuery query)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized access");
            }

            if (query.MuseumId != null)
            {
                var museum = _museumRepository.GetById(query.MuseumId.Value);
                if (museum == null)
                {
                    return ErrorResp.NotFound("Museum not found");
                }
                var user = await _userRepository.GetByIdAsync(payload.UserId);
                if (user == null)
                {
                    return ErrorResp.NotFound("User not found");
                }
                // if (!payload.IsAdmin)
                // {
                    //check owner
                    if (museum.CreatedBy != payload.UserId)
                    {
                        return ErrorResp.Unauthorized("You are not the owner of this museum");
                    }
                // }
            }

            var events = await _eventRepository.GetAllAdminAsync(query);
            var eventDtos = _mapper.Map<List<EventDto>>(events.Events);
            return SuccessResp.Ok(new { List = eventDtos, Total = events.Total });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleAddArtifactToEventSameMuseum(Guid eventId, List<Guid> artifactIds)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized access");
            }

            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            // if (!payload.IsAdmin)
            // {
                //check owner
                if (eventItem.CreatedBy != payload.UserId)
                {
                    return ErrorResp.Unauthorized("You are not the owner of this event");
                }
            // }

            foreach (var artifactId in artifactIds)
            {
                var artifact = await _artifactRepository.GetByIdAsync(artifactId);
                if (artifact == null)
                {
                    return ErrorResp.NotFound("Some artifacts not found");
                }
                if (artifact.MuseumId != eventItem.MuseumId)
                {
                    return ErrorResp.BadRequest("Some artifacts are not in the same museum");
                }
                if (!artifact.IsActive)
                {
                    return ErrorResp.BadRequest("Some artifacts are not active");
                }
                eventItem.Artifacts.Add(artifact);
            }

            await _eventRepository.UpdateAsync(eventId, eventItem);
            return SuccessResp.Ok("Artifacts added successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleEvaluateEvent(Guid id, bool isApproved)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized access");
            }

            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            // if (!payload.IsAdmin)
            // {
                //check owner
                if (eventItem.CreatedBy != payload.UserId)
                {
                    return ErrorResp.Unauthorized("You are not the owner of this event");
                }
            // }

            if (eventItem.Status != EventStatusEnum.Pending && eventItem.Status != EventStatusEnum.Draft)
            {
                return ErrorResp.BadRequest("Event is not in pending or draft status");
            }

            eventItem.Status = isApproved ? EventStatusEnum.Published : EventStatusEnum.Cancelled;
            await _eventRepository.UpdateAsync(id, eventItem);
            return SuccessResp.Ok("Event evaluated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleUpdateAdmin(Guid id, EventUpdateDto dto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized access");
            }

            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            // if (!payload.IsAdmin)
            // {
                //check owner
                if (eventItem.CreatedBy != payload.UserId)
                {
                    return ErrorResp.Unauthorized("You are not the owner of this event");
                }
            // }

            var mappedEvent = _mapper.Map(dto, eventItem);
            await _eventRepository.UpdateAsync(id, mappedEvent);
            return SuccessResp.Ok("Event updated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleDeleteAdmin(Guid id)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized access");
            }

            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            // if (!payload.IsAdmin)
            // {
                //check owner
                if (eventItem.CreatedBy != payload.UserId)
                {
                    return ErrorResp.Unauthorized("You are not the owner of this event");
                }
            // }

            await _eventRepository.DeleteAsync(id);
            return SuccessResp.Ok("Event deleted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleCreateAdmin(Guid museumId, EventCreateAdminDto dto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized access");
            }

            var museum = _museumRepository.GetById(museumId);
            if (museum == null)
            {
                return ErrorResp.NotFound("Museum not found");
            }

            // if (!payload.IsAdmin)
            // {
                //check owner
                if (museum.CreatedBy != payload.UserId)
                {
                    return ErrorResp.Unauthorized("You are not the owner of this museum");
                }
            // }

            var eventItem = _mapper.Map<Event>(dto);
            //check field null
            if (eventItem.Description == null)
            {
                eventItem.Description = "";
            }
            if (eventItem.Location == null)
            {
                eventItem.Location = "";
            }
            eventItem.CreatedBy = payload.UserId;
            eventItem.MuseumId = museumId;

            await _eventRepository.AddAsync(eventItem);
            var eventDto = _mapper.Map<EventDto>(eventItem);
            return SuccessResp.Created(eventDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}

// Organizer implementation
public class OrganizerEventService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseEventService(context, mapper, httpContextAccessor), IOrganizerEventService
{
    public async Task<IActionResult> HandleCreateDraft(Guid museumId, EventCreateDto dto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var isMuseumExists = _museumRepository.IsMuseumExists(museumId);
            if (!isMuseumExists)
            {
                return ErrorResp.NotFound("Museum not found");
            }

            var eventItem = _mapper.Map<Event>(dto);
            eventItem.CreatedBy = payload.UserId;
            eventItem.MuseumId = museumId;
            eventItem.Status = EventStatusEnum.Draft;

            await _eventRepository.AddAsync(eventItem);
            var eventDto = _mapper.Map<EventDto>(eventItem);
            return SuccessResp.Created(eventDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleSubmitEvent(Guid id)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            if (eventItem.CreatedBy != payload.UserId)
            {
                return ErrorResp.Unauthorized("You are not the owner of this event");
            }

            if (eventItem.Status != EventStatusEnum.Draft)
            {
                return ErrorResp.BadRequest("Event is not in draft status");
            }

            eventItem.Status = EventStatusEnum.Pending;
            await _eventRepository.UpdateAsync(id, eventItem);
            return SuccessResp.Ok("Event submitted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetDraft()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var events = await _eventRepository.GetDraftEventOrganizerAsync(payload.UserId);
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetSubmitted()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var events = await _eventRepository.GetSubmittedEventOrganizerAsync(payload.UserId);
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetExpired()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var events = await _eventRepository.GetExpiredEventOrganizerAsync(payload.UserId);
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAllByOrganizer()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var events = await _eventRepository.GetAllEventByOrganizerAsync(payload.UserId);
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleUpdate(Guid id, EventUpdateDto dto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            // if (eventItem.CreatedBy != payload.UserId)
            // {
            //     return ErrorResp.Unauthorized("You are not the authorized for this event");
            // }

            if (eventItem.Status != EventStatusEnum.Draft && eventItem.Status != EventStatusEnum.Pending)
            {
                return ErrorResp.BadRequest("Event is not in draft or pending status");
            }

            var mappedEvent = _mapper.Map(dto, eventItem);
            await _eventRepository.UpdateAsync(id, mappedEvent);
            return SuccessResp.Ok("Event updated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleDelete(Guid id)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }

            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            // if (eventItem.CreatedBy != payload.UserId)
            // {
            //     return ErrorResp.Unauthorized("You are not the authorized for this event");
            // }

            if (eventItem.Status != EventStatusEnum.Draft && eventItem.Status != EventStatusEnum.Pending)
            {
                return ErrorResp.BadRequest("Event is not in draft or pending status");
            }

            await _eventRepository.DeleteAsync(id);
            return SuccessResp.Ok("Event deleted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}
