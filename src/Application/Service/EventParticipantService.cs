using Application.Service;
using Application.Shared.Enum;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Events;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface IEventParticipantService
{
    Task<IActionResult> HandleGetById(Guid id);
    Task<IActionResult> HandleGetAll();
    Task<IActionResult> HandleGetByEventId(Guid eventId);
    Task<IActionResult> HandleGetByUserId(Guid userId);
    Task<IActionResult> HandleGetByEventIdAndUserId(Guid eventId, Guid userId);
    Task<IActionResult> HandleAdd(EventParticipantCreateDto eventParticipant);
    Task<IActionResult> HandleUpdate(Guid eventParticipantId, EventParticipantUpdateDto eventParticipant);
    Task<IActionResult> HandleDelete(Guid eventParticipantId);
    Task<IActionResult> HandleAddClientEvent(Guid userId, Guid eventId);
}
public class EventParticipantService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService(context, mapper, httpContextAccessor), IEventParticipantService
{
    private readonly IEventParticipantRepository _eventParticipantRepository = new EventParticipantRepository(context);
    private readonly IEventRepository _eventRepository = new EventRepository(context);
    private readonly IUserRepository _userRepository = new UserRepository(context);

    public async Task<IActionResult> HandleAdd(EventParticipantCreateDto eventParticipant)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }

            // check if the event exists
            var isEventExists = await _eventRepository.GetEventById(eventParticipant.EventId);
            if (isEventExists == null)
            {
                return ErrorResp.NotFound("Event not found");
            }

            // check if the user exists
            var isUserExists = await _userRepository.GetByIdAsync(eventParticipant.UserId);
            if (isUserExists == null)
            {
                return ErrorResp.NotFound("User not found");
            }

            // check if participant already exists for this event and user
            var existingParticipant = await _eventParticipantRepository.GetByEventIdAndUserIdAsync(eventParticipant.EventId, eventParticipant.UserId);
            if (existingParticipant != null)
            {
                return ErrorResp.BadRequest("User is already a participant in this event");
            }

            var eventParticipantItem = mapper.Map<EventParticipant>(eventParticipant);
            eventParticipantItem.JoinedAt = DateTime.UtcNow;

            await _eventParticipantRepository.AddAsync(eventParticipantItem);
            var eventParticipantDto = _mapper.Map<EventParticipantDto>(eventParticipantItem);
            return SuccessResp.Created(eventParticipantDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleDelete(Guid eventParticipantId)
    {
        try
        {
            var eventParticipant = await _eventParticipantRepository.GetByIdAsync(eventParticipantId);
            if (eventParticipant == null)
            {
                return ErrorResp.NotFound("Event participant not found");
            }
            await _eventParticipantRepository.DeleteAsync(eventParticipant);
            return SuccessResp.Ok("Event participant deleted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAll()
    {
        try
        {
            var eventParticipants = await _eventParticipantRepository.GetAllAsync();
            var eventParticipantsDto = mapper.Map<List<EventParticipantDto>>(eventParticipants);
            return SuccessResp.Ok(eventParticipantsDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetByEventId(Guid eventId)
    {
        try
        {
            var eventParticipants = await _eventParticipantRepository.GetByEventIdAsync(eventId);
            var eventParticipantsDto = mapper.Map<List<EventParticipantDto>>(eventParticipants);
            return SuccessResp.Ok(eventParticipantsDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetByEventIdAndUserId(Guid eventId, Guid userId)
    {
        try
        {
            var eventParticipant = await _eventParticipantRepository.GetByEventIdAndUserIdAsync(eventId, userId);
            if (eventParticipant == null)
            {
                return ErrorResp.NotFound("Event participant not found");
            }
            var eventParticipantDto = mapper.Map<EventParticipantDto>(eventParticipant);
            return SuccessResp.Ok(eventParticipantDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetById(Guid id)
    {
        try
        {
            var eventParticipant = await _eventParticipantRepository.GetByIdAsync(id);
            if (eventParticipant == null)
            {
                return ErrorResp.NotFound("Event participant not found");
            }
            var eventParticipantDto = mapper.Map<EventParticipantDto>(eventParticipant);
            return SuccessResp.Ok(eventParticipantDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetByUserId(Guid userId)
    {
        try
        {
            var eventParticipants = await _eventParticipantRepository.GetByUserIdAsync(userId);
            var eventParticipantsDto = mapper.Map<List<EventParticipantDto>>(eventParticipants);
            return SuccessResp.Ok(eventParticipantsDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleUpdate(Guid eventParticipantId, EventParticipantUpdateDto updateDto)
    {
        try
        {
            var eventParticipantItem = await _eventParticipantRepository.GetByIdAsync(eventParticipantId);
            if (eventParticipantItem == null)
            {
                return ErrorResp.NotFound("Event participant not found");
            }
            var eventParticipantToUpdate = mapper.Map(updateDto, eventParticipantItem);
            await _eventParticipantRepository.UpdateAsync(eventParticipantId, eventParticipantToUpdate);
            var eventParticipantDto = mapper.Map<EventParticipantDto>(eventParticipantToUpdate);
            return SuccessResp.Ok(eventParticipantDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleAddClientEvent(Guid userId, Guid eventId)
    {
        try
        {
            var isEventExists = await _eventRepository.GetEventById(eventId);
            if (isEventExists == null)
            {
                return ErrorResp.NotFound("Event not found");
            }
            var isUserExists = await _userRepository.GetByIdAsync(userId);
            if (isUserExists == null)
            {
                return ErrorResp.NotFound("User not found");
            }

            // check if participant already exists for this event and user  
            var existingParticipant = await _eventParticipantRepository.GetByEventIdAndUserIdAsync(eventId, userId);
            if (existingParticipant != null && existingParticipant.Role == ParticipantRoleEnum.Attendee)
            {
                return ErrorResp.BadRequest("User is already a participant in this event");
            }

            var eventParticipant = new EventParticipant
            {
                UserId = userId,
                EventId = eventId,
                JoinedAt = DateTime.UtcNow,
                Role = ParticipantRoleEnum.Attendee,
                Status = ParticipantStatusEnum.Confirmed
            };
            await _eventParticipantRepository.AddAsync(eventParticipant);
            return SuccessResp.Created("Add client to event successful");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}