using Application.Service;
using Application.Shared.Enum;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Events;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using MuseTrip360.src.Infrastructure.Repository;

public interface IEventService
{
    //normal user
    Task<IActionResult> HandleGetEventById(Guid id);
    Task<IActionResult> HandleGetAll(EventQuery query);
    Task<IActionResult> HandleGetEventsByMuseumId(Guid museumId);
    //manager //admin
    Task<IActionResult> HandleGetAllAdmin(EventAdminQuery query);
    Task<IActionResult> HandleAddArtifactToEventSameMuseum(Guid eventId, List<Guid> artifactIds);
    Task<IActionResult> HandleEvaluateEvent(Guid id, bool isApproved);
    // organizer
    Task<IActionResult> HandleCreateByOrganizer(Guid museumId, EventCreateDto dto);
    Task<IActionResult> HandleSubmitEvent(Guid id);
    Task<IActionResult> HandleGetDraftEventByOrganizer();
    Task<IActionResult> HandleGetSubmmittedEventByOrganizer();
    Task<IActionResult> HandleGetExpiredEventByOrganizer();
    Task<IActionResult> HandleGetAllEventByOrganizer();
    Task<IActionResult> HandleUpdate(Guid id, EventUpdateDto dto);
    Task<IActionResult> HandleDelete(Guid id);

}

public class EventService : BaseService, IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMuseumRepository _museumRepository;
    private readonly IArtifactRepository _artifactRepository;

    public EventService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    : base(context, mapper, httpContextAccessor)
    {
        _eventRepository = new EventRepository(context);
        _museumRepository = new MuseumRepository(context);
        _artifactRepository = new ArtifactRepository(context);
    }

    public async Task<IActionResult> HandleCreate(Guid museumId, EventCreateDto dto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }
            // check if the museum is exists
            var isMuseumExists = _museumRepository.IsMuseumExists(museumId);
            if (!isMuseumExists)
            {
                return ErrorResp.NotFound("Museum not found");
            }
            // map the dto to the event
            var eventItem = _mapper.Map<Event>(dto);
            eventItem.CreatedBy = payload.UserId;
            eventItem.MuseumId = museumId;
            eventItem.Status = EventStatusEnum.Draft;
            // create the event
            await _eventRepository.AddAsync(eventItem);
            var eventDto = _mapper.Map<EventDto>(eventItem);
            return SuccessResp.Created(eventDto);
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
                return ErrorResp.Unauthorized("Invalid token");
            }
            // check if the event is exists
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }
            // check if user is admin or owner of the event
            if (eventItem.Museum.CreatedBy != payload.UserId && payload.IsAdmin == false)
            {
                return ErrorResp.Unauthorized("You are not admin or the owner of this event");
            }
            // delete the event
            await _eventRepository.DeleteAsync(id);
            return SuccessResp.Ok("Event deleted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAllAdmin(EventAdminQuery query)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }
            // check if query museumId is null or not
            if (query.MuseumId != null)
            {
                var museum = _museumRepository.GetById(query.MuseumId.Value);
                if (museum == null)
                {
                    return ErrorResp.NotFound("Museum not found");
                }
                // check if the user is the owner of the museum
                if (museum.CreatedBy != payload.UserId)
                {
                    return ErrorResp.Unauthorized("You are not the owner of this museum");
                }
            }
            // get all events
            var events = await _eventRepository.GetAllAdminAsync(query);
            // map the events to the event dtos
            var eventDtos = _mapper.Map<List<EventDto>>(events.Events);
            // return the event dtos
            return SuccessResp.Ok(new
            {
                List = eventDtos,
                Total = events.Total
            });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAll(EventQuery query)
    {
        try
        {
            // get all events
            var events = await _eventRepository.GetAllAsync(query);
            // map the events to the event dtos
            var eventDtos = _mapper.Map<List<EventDto>>(events.Events);
            // return the event dtos
            return SuccessResp.Ok(new
            {
                List = eventDtos,
                Total = events.Total
            });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetEventById(Guid id)
    {
        try
        {
            // get the event
            var eventItem = await _eventRepository.GetEventById(id);
            // map the event to the event dto
            var eventDto = _mapper.Map<EventDto>(eventItem);
            return SuccessResp.Ok(eventDto);
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
                return ErrorResp.Unauthorized("Invalid token");
            }
            // get the event
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }
            // check if user is admin or owner of the event
            if (eventItem.Museum.CreatedBy != payload.UserId && payload.IsAdmin == false)
            {
                return ErrorResp.Unauthorized("You are not admin or the owner of this event");
            }
            // map the dto to the event
            var mappedEvent = _mapper.Map(dto, eventItem);
            // update the event
            await _eventRepository.UpdateAsync(id, mappedEvent);
            // return the success response
            return SuccessResp.Ok("Event updated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetEventsByMuseumId(Guid museumId)
    {
        try
        {
            // get the events
            var events = await _eventRepository.GetEventsByMuseumIdAsync(museumId);
            // map the events to the event dtos
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            // return the event dtos
            return SuccessResp.Ok(eventDtos);
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
            // get the event
            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }
            // get the artifacts
            foreach (var artifactId in artifactIds)
            {
                var artifactItem = await _artifactRepository.GetByIdAsync(artifactId);
                if (artifactItem == null)
                {
                    return ErrorResp.NotFound("Some artifacts not found");
                }
                if (artifactItem.MuseumId != eventItem.MuseumId)
                {
                    return ErrorResp.BadRequest("Some artifacts are not in the same museum");
                }
                // if (eventItem.Artifacts.Any(a => a.Status != Ready))
                // {
                //     return ErrorResp.BadRequest("Some artifacts are already in the event");
                // }
                if (artifactItem.IsActive == false)
                {
                    return ErrorResp.BadRequest("Some artifacts are not active");
                }
                // add the artifact to the event
                eventItem.Artifacts.Add(artifactItem);
                // queue the artifact to be updated
            }
            // update the event
            await _eventRepository.UpdateAsync(eventId, eventItem);
            // return the success response
            return SuccessResp.Ok("Artifacts added to event successfully");
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
            // get the event
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }
            // check if the user is the owner of the museum
            if (eventItem.Museum.CreatedBy != payload.UserId)
            {
                return ErrorResp.Unauthorized("You are not the owner of this museum");
            }
            if (eventItem.Status != EventStatusEnum.Draft)
            {
                return ErrorResp.BadRequest("Event is not in draft status");
            }
            // submit the event
            eventItem.Status = EventStatusEnum.Pending;
            // update the event
            await _eventRepository.UpdateAsync(id, eventItem);
            // return the success response
            return SuccessResp.Ok("Event submitted successfully");
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
                return ErrorResp.Unauthorized("Invalid token");
            }
            // get the event
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }
            // check if the user is the owner of the museum
            if (eventItem.Museum.CreatedBy != payload.UserId)
            {
                return ErrorResp.Unauthorized("You are not the owner of this museum");
            }
            if (eventItem.Status != EventStatusEnum.Pending)
            {
                return ErrorResp.BadRequest("Event is not in pending status");
            }
            // evaluate the event
            eventItem.Status = isApproved ? EventStatusEnum.Published : EventStatusEnum.Cancelled;
            // update the event
            await _eventRepository.UpdateAsync(id, eventItem);
            // return the success response
            return SuccessResp.Ok("Event evaluated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetDraftEventOrganizer()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }
            // get the events
            var events = await _eventRepository.GetDraftEventOrganizerAsync(payload.UserId);
            // map the events to the event dtos
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            // return the event dtos
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetSubmmittedEventOrganizer()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }
            // get the events
            var events = await _eventRepository.GetSubmittedEventOrganizerAsync(payload.UserId);
            // map the events to the event dtos
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            // return the event dtos
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleCreateByOrganizer(Guid museumId, EventCreateDto dto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }
            // check if the museum is exists
            var isMuseumExists = _museumRepository.IsMuseumExists(museumId);
            if (!isMuseumExists)
            {
                return ErrorResp.NotFound("Museum not found");
            }
            // map the dto to the event
            var eventItem = _mapper.Map<Event>(dto);
            eventItem.CreatedBy = payload.UserId;
            eventItem.MuseumId = museumId;
            eventItem.Status = EventStatusEnum.Draft;
            // create the event
            await _eventRepository.AddAsync(eventItem);
            var eventDto = _mapper.Map<EventDto>(eventItem);
            return SuccessResp.Created(eventDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetDraftEventByOrganizer()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }
            // get the events
            var events = await _eventRepository.GetDraftEventOrganizerAsync(payload.UserId);
            // map the events to the event dtos
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            // return the event dtos
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetSubmmittedEventByOrganizer()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }
            // get the events
            var events = await _eventRepository.GetSubmittedEventOrganizerAsync(payload.UserId);
            // map the events to the event dtos
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            // return the event dtos
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetExpiredEventByOrganizer()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }
            // get the events
            var events = await _eventRepository.GetExpiredEventOrganizerAsync(payload.UserId);
            // map the events to the event dtos
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            // return the event dtos
            return SuccessResp.Ok(eventDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAllEventByOrganizer()
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Invalid token");
            }
            // get the events
            var events = await _eventRepository.GetAllEventByOrganizerAsync(payload.UserId);
            // map the events to the event dtos
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            // return the event dtos
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
            // get the event
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }
            if (!payload.IsAdmin && eventItem.CreatedBy != payload.UserId)
            {
                if (eventItem.CreatedBy != payload.UserId)
                {
                    return ErrorResp.Unauthorized("You are not the owner of this event");
                }
                // check if the event is in draft or pending status
                if (eventItem.Status != EventStatusEnum.Draft && eventItem.Status != EventStatusEnum.Pending)
                {
                    return ErrorResp.BadRequest("Event is not in draft or pending status");
                }
            }
            // check if the user is the owner of the event
            // map the dto to the event
            var mappedEvent = _mapper.Map(dto, eventItem);
            // update the event
            await _eventRepository.UpdateAsync(id, mappedEvent);
            // return the success response
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
            // check if the event is exists
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }
            if (!payload.IsAdmin && eventItem.CreatedBy != payload.UserId)
            {
                if (eventItem.CreatedBy != payload.UserId)
                {
                    return ErrorResp.Unauthorized("You are not the owner of this event");
                }
                // check if the event is in draft or pending status
                if (eventItem.Status != EventStatusEnum.Draft && eventItem.Status != EventStatusEnum.Pending)
                {
                    return ErrorResp.BadRequest("Event is not in draft or pending status");
                }
            }
            // delete the event
            await _eventRepository.DeleteAsync(id);
            return SuccessResp.Ok("Event deleted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}
