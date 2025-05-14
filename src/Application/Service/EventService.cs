using Application.Service;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Events;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using MuseTrip360.src.Infrastructure.Repository;

public interface IEventService
{
    Task<IActionResult> HandleGetEventById(Guid id);
    Task<IActionResult> HandleGetAll(EventQuery query);
    Task<IActionResult> HandleGetAllAdmin(EventAdminQuery query);
    Task<IActionResult> HandleCreate(Guid museumId, EventCreateDto dto);
    Task<IActionResult> HandleUpdate(Guid id, EventUpdateDto dto);
    Task<IActionResult> HandleDelete(Guid id);
    Task<IActionResult> HandleGetEventsByMuseumId(Guid museumId);
    Task<IActionResult> HandleAddArtifactToEventSameMuseum(Guid eventId, List<Guid> artifactIds);
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

    public async Task<IActionResult> HandleDelete(Guid id)
    {
        try
        {
            // check if the event is exists
            var isEventExists = await _eventRepository.IsEventExistsAsync(id);
            if (!isEventExists)
            {
                return ErrorResp.NotFound("Event not found");
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

    public async Task<IActionResult> HandleUpdate(Guid id, EventUpdateDto dto)
    {
        try
        {
            // get the event
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
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
}
