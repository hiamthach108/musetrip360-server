using Application.Service;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

public interface IRoomService
{
    Task<IActionResult> HandleCreate(Guid eventId, RoomCreateDto dto);
    Task<IActionResult> HandleGetById(string id);
    Task<IActionResult> HandleUpdate(string id, RoomUpdateDto dto);
    Task<IActionResult> HandleDelete(string id);
    Task<IActionResult> HandleUpdateMetadata(string id, RoomUpdateMetadataDto dto);
    Task<bool> ValidateUser(Guid userId, string roomId);
    Task<IActionResult> HandleGetRoomByEventId(Guid eventId);
    Task<Room> GetRoomById(string id);
}

public class RoomService(MuseTrip360DbContext dbContext, IMapper mapper, IConnectionMultiplexer redisConnection, IHttpContextAccessor httpContextAccessor) : BaseService(dbContext, mapper, httpContextAccessor), IRoomService
{
    private readonly IRoomRepository _roomRepository = new RoomRepository(redisConnection);
    private readonly IEventRepository _eventRepository = new EventRepository(dbContext);
    private readonly IEventParticipantRepository _eventParticipantRepository = new EventParticipantRepository(dbContext);

    public async Task<IActionResult> HandleCreate(Guid eventId, RoomCreateDto dto)
    {
        try
        {
            var room = _mapper.Map<Room>(dto);
            var isEventExists = await _eventRepository.IsEventExistsAsync(eventId);
            if (!isEventExists)
            {
                return ErrorResp.NotFound("Event not found");
            }
            room.EventId = eventId;
            // create the room
            await _roomRepository.CreateRoom(room);
            return SuccessResp.Created(room);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleDelete(string id)
    {
        try
        {
            var room = await _roomRepository.GetRoomById(id);
            if (room == null)
            {
                return ErrorResp.NotFound("Room not found");
            }
            await _roomRepository.DeleteRoom(id);
            return SuccessResp.Ok("Room deleted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetById(string id)
    {
        try
        {
            var room = await _roomRepository.GetRoomById(id);
            if (room == null)
            {
                return ErrorResp.NotFound("Room not found");
            }
            var roomDto = _mapper.Map<RoomDto>(room);
            return SuccessResp.Ok(roomDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleUpdate(string id, RoomUpdateDto dto)
    {
        try
        {
            var room = await _roomRepository.GetRoomById(id);
            if (room == null)
            {
                return ErrorResp.NotFound("Room not found");
            }
            var roomToUpDate = _mapper.Map(dto, room);
            await _roomRepository.UpdateRoom(id, roomToUpDate);
            return SuccessResp.Ok("Room updated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleUpdateMetadata(string id, RoomUpdateMetadataDto dto)
    {
        try
        {
            var room = await _roomRepository.GetRoomById(id);
            if (room == null)
            {
                return ErrorResp.NotFound("Room not found");
            }
            room.Metadata = dto.Metadata;
            await _roomRepository.UpdateRoom(id, room);
            return SuccessResp.Ok("Room updated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<bool> ValidateUser(Guid userId, string roomId)
    {
        var room = await _roomRepository.GetRoomById(roomId);
        if (room == null)
        {
            return false;
        }
        if (!await _eventParticipantRepository.ValidateUser(userId, room.EventId))
        {
            return false;
        }
        return true;
    }

    public async Task<IActionResult> HandleGetRoomByEventId(Guid eventId)
    {
        var room = await _roomRepository.GetRoomByEventId(eventId);
        if (room == null)
        {
            return ErrorResp.NotFound("Room not found");
        }
        return SuccessResp.Ok(room);
    }

    public async Task<Room> GetRoomById(string id)
    {
        try
        {
            var room = await _roomRepository.GetRoomById(id);
            if (room == null)
            {
                throw new Exception("Room not found");
            }
            return room;
        }
        catch
        {
            throw;
        }
    }
}
