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
using StackExchange.Redis;

// Normal user operations
public interface IEventService
{
    Task<IActionResult> HandleGetEventById(Guid id);
    Task<IActionResult> HandleGetAll(EventQuery query);
    Task<IActionResult> HandleGetEventsByMuseumId(Guid museumId, EventAdminQuery query);
    Task<bool> IsOwner(Guid userId, Guid eventId);
    Task<IActionResult> HandleFeedback(Guid id, string comment);
}

// Admin, Manager operations
public interface IAdminEventService : IEventService
{
    Task<IActionResult> HandleGetAllAdmin(EventAdminQuery query);
    Task<IActionResult> HandleAddArtifactToEvent(Guid eventId, IEnumerable<Guid> artifactIds);
    Task<IActionResult> HandleRemoveArtifactFromEvent(Guid eventId, IEnumerable<Guid> artifactIds);
    Task<IActionResult> HandleEvaluateEvent(Guid id, bool isApproved);
    Task<IActionResult> HandleUpdateAdmin(Guid id, EventUpdateDto dto);
    Task<IActionResult> HandleDeleteAdmin(Guid id);
    Task<IActionResult> HandleCreateAdmin(Guid museumId, EventCreateAdminDto dto);
    Task<IActionResult> HandleCancelEvent(Guid id);
    Task<IActionResult> HandleAddTourOnlineToEvent(Guid eventId, IEnumerable<Guid> tourOnlineIds);
    Task<IActionResult> HandleRemoveTourOnlineFromEvent(Guid eventId, IEnumerable<Guid> tourOnlineIds);
    Task<IActionResult> HandleAddTourGuideToEvent(Guid eventId, IEnumerable<Guid> tourGuideIds);
    Task<IActionResult> HandleRemoveTourGuideFromEvent(Guid eventId, IEnumerable<Guid> tourGuideIds);
}

// Organizer, operations
public interface IOrganizerEventService : IEventService
{
    Task<IActionResult> HandleCreateDraft(Guid museumId, EventCreateDto dto);
    Task<IActionResult> HandleSubmitEvent(Guid id);
    Task<IActionResult> HandleGetAllByOrganizer(EventStatusEnum? status);
    Task<IActionResult> HandleUpdate(Guid id, EventUpdateDto dto);
    Task<IActionResult> HandleDelete(Guid id);
}

// Base implementation for common operations
public abstract class BaseEventService(MuseTrip360DbContext context, IConnectionMultiplexer redisConnection, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService(context, mapper, httpContextAccessor), IEventService
{
    protected readonly IEventRepository _eventRepository = new EventRepository(context);
    protected readonly IMuseumRepository _museumRepository = new MuseumRepository(context);
    private readonly IRoomRepository _roomRepository = new RoomRepository(redisConnection);

    public virtual async Task<IActionResult> HandleGetEventById(Guid id)
    {
        try
        {
            var eventItem = await _eventRepository.GetEventById(id);
            var roomItems = await _roomRepository.GetRoomByEventId(id);
            var eventDto = _mapper.Map<EventDto>(eventItem);
            var roomDtos = _mapper.Map<List<RoomDto>>(roomItems);
            // attach rooms to event
            eventDto.Rooms = roomDtos;
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

    public virtual async Task<IActionResult> HandleGetEventsByMuseumId(Guid museumId, EventAdminQuery query)
    {
        try
        {
            var events = await _eventRepository.GetEventsByMuseumIdAsync(museumId, query);
            var eventDtos = _mapper.Map<List<EventDto>>(events.Events);
            return SuccessResp.Ok(new { List = eventDtos, Total = events.Total });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<bool> IsOwner(Guid userId, Guid eventId)
    {
        return await _eventRepository.IsOwner(userId, eventId);
    }

    public async Task<IActionResult> HandleFeedback(Guid id, string comment)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }
            // check if the event is exists
            var isEventExists = await _eventRepository.IsEventExistsAsync(id);
            if (!isEventExists)
            {
                return ErrorResp.NotFound("Event not found");
            }
            // rate the event
            await _eventRepository.FeedbackEvents(id, payload.UserId, comment);
            return SuccessResp.Ok("Event rated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}

public class EventService(MuseTrip360DbContext context, IConnectionMultiplexer redisConnection, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseEventService(context, redisConnection, mapper, httpContextAccessor), IEventService
{
}

// Admin implementation
public class AdminEventService(MuseTrip360DbContext context, IConnectionMultiplexer redisConnection, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseEventService(context, redisConnection, mapper, httpContextAccessor), IAdminEventService
{
    protected readonly IArtifactRepository _artifactRepository = new ArtifactRepository(context);
    protected readonly ITourOnlineRepository _tourOnlineRepository = new TourOnlineRepository(context);
    protected readonly ITourGuideRepository _tourGuideRepository = new TourGuideRepository(context);
    public async Task<IActionResult> HandleGetAllAdmin(EventAdminQuery query)
    {
        try
        {
            if (query.MuseumId != null)
            {
                var museum = _museumRepository.GetById(query.MuseumId.Value);
                if (museum == null)
                {
                    return ErrorResp.NotFound("Museum not found");
                }
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

    public async Task<IActionResult> HandleAddArtifactToEvent(Guid eventId, IEnumerable<Guid> artifactIds)
    {
        try
        {
            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            // get artifacts that are in the same museum and active
            var artifactList = await _artifactRepository.GetArtifactByListIdMuseumIdStatus(artifactIds, eventItem.MuseumId, true);
            if (!artifactList.IsAllFound)
            {
                return ErrorResp.BadRequest($"Some artifacts are not found, not in the same museum, or not available: {string.Join(", ", artifactList.MissingIds)}");
            }
            foreach (var artifact in artifactList.Artifacts)
            {
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
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            if (eventItem.Status != EventStatusEnum.Pending && eventItem.Status != EventStatusEnum.Draft)
            {
                return ErrorResp.BadRequest("Event is not in pending or draft status");
            }

            eventItem.Status = isApproved ? EventStatusEnum.Published : EventStatusEnum.Draft;
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
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
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

    public async Task<IActionResult> HandleDeleteAdmin(Guid id)
    {
        try
        {
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

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
                return ErrorResp.Unauthorized("Unauthorized");
            }
            var museum = _museumRepository.GetById(museumId);
            if (museum == null)
            {
                return ErrorResp.NotFound("Museum not found");
            }

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

    public async Task<IActionResult> HandleRemoveArtifactFromEvent(Guid eventId, IEnumerable<Guid> artifactIds)
    {
        try
        {

            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            var artifactList = await _artifactRepository.GetArtifactByListIdEventId(artifactIds, eventId);
            if (!artifactList.IsAllFound)
            {
                return ErrorResp.BadRequest($"Some artifacts are not found, not in the same museum, or not available: {string.Join(", ", artifactList.MissingIds)}");
            }
            foreach (var artifact in artifactList.Artifacts)
            {
                eventItem.Artifacts.Remove(artifact);
            }

            await _eventRepository.UpdateAsync(eventId, eventItem);
            return SuccessResp.Ok("Artifacts removed from event successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleCancelEvent(Guid id)
    {
        try
        {
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            eventItem.Status = EventStatusEnum.Cancelled;
            await _eventRepository.UpdateAsync(id, eventItem);
            return SuccessResp.Ok("Event cancelled successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleAddTourOnlineToEvent(Guid eventId, IEnumerable<Guid> tourOnlineIds)
    {
        try
        {
            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            var tourOnlineList = await _tourOnlineRepository.GetTourOnlineByListIdMuseumIdStatus(tourOnlineIds, eventItem.MuseumId, true);
            if (!tourOnlineList.IsAllFound)
            {
                return ErrorResp.BadRequest($"Some tour online are not found, not in the same museum, or not available: {string.Join(", ", tourOnlineList.MissingIds)}");
            }
            foreach (var tourOnline in tourOnlineList.Tours)
            {
                eventItem.TourOnlines.Add(tourOnline);
            }

            await _eventRepository.UpdateAsync(eventId, eventItem);
            return SuccessResp.Ok("Tour online added to event successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleRemoveTourOnlineFromEvent(Guid eventId, IEnumerable<Guid> tourOnlineIds)
    {
        try
        {

            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            var tourOnlineList = await _tourOnlineRepository.GetTourOnlineByListIdEventId(tourOnlineIds, eventId);
            if (!tourOnlineList.IsAllFound)
            {
                return ErrorResp.BadRequest($"Some tour online are not found, not in the same museum, or not available: {string.Join(", ", tourOnlineList.MissingIds)}");
            }
            foreach (var tourOnline in tourOnlineList.Tours)
            {
                eventItem.TourOnlines.Remove(tourOnline);
            }

            await _eventRepository.UpdateAsync(eventId, eventItem);
            return SuccessResp.Ok("Tour online removed from event successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleAddTourGuideToEvent(Guid eventId, IEnumerable<Guid> tourGuideIds)
    {
        try
        {
            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            var tourGuideList = await _tourGuideRepository.GetTourGuideByListIdEventIdStatus(tourGuideIds, eventId, true);
            if (!tourGuideList.IsAllFound)
            {
                return ErrorResp.BadRequest($"Some tour guides are not found, not in the same museum, or not available: {string.Join(", ", tourGuideList.MissingIds)}");
            }
            foreach (var tourGuide in tourGuideList.TourGuides)
            {
                eventItem.TourGuides.Add(tourGuide);
            }

            await _eventRepository.UpdateAsync(eventId, eventItem);
            return SuccessResp.Ok("Tour guide added to event successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleRemoveTourGuideFromEvent(Guid eventId, IEnumerable<Guid> tourGuideIds)
    {
        try
        {
            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            var tourGuideList = await _tourGuideRepository.GetTourGuideByListIdEventIdStatus(tourGuideIds, eventId, true);
            if (!tourGuideList.IsAllFound)
            {
                return ErrorResp.BadRequest($"Some tour guides are not found, not in the same museum, or not available: {string.Join(", ", tourGuideList.MissingIds)}");
            }
            foreach (var tourGuide in tourGuideList.TourGuides)
            {
                eventItem.TourGuides.Remove(tourGuide);
            }

            await _eventRepository.UpdateAsync(eventId, eventItem);
            return SuccessResp.Ok("Tour guide removed from event successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}

// Organizer implementation
public class OrganizerEventService(MuseTrip360DbContext context, IConnectionMultiplexer redisConnection, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseEventService(context, redisConnection, mapper, httpContextAccessor), IOrganizerEventService
{
    public async Task<IActionResult> HandleCreateDraft(Guid museumId, EventCreateDto dto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized");
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
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
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

    public async Task<IActionResult> HandleGetAllByOrganizer(EventStatusEnum? status)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }
            var events = await _eventRepository.GetAllEventByOrganizerAsync(payload.UserId, status);
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
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

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
            var eventItem = await _eventRepository.GetEventById(id);
            if (eventItem == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

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
