using StackExchange.Redis;

public interface IRoomStateManager
{
    Task<Room?> GetRoomState(string roomId);
    Task UpdateRoomState(string roomId, RoomUpdateMetadataDto dto);
}

public class RoomStateManager : IRoomStateManager
{
    private readonly IRoomRepository _roomRepository;
    public RoomStateManager(IConnectionMultiplexer redisConnection)
    {
        _roomRepository = new RoomRepository(redisConnection);
    }
    public async Task<Room?> GetRoomState(string roomId)
    {
        try
        {
            var room = await _roomRepository.GetRoomById(roomId);
            return room;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task UpdateRoomState(string roomId, RoomUpdateMetadataDto dto)
    {
        try
        {
            var room = await _roomRepository.GetRoomById(roomId);
            if (room == null)
            {
                throw new Exception("Room not found");
            }
            room.Metadata = dto.Metadata;
            await _roomRepository.UpdateRoom(roomId, room);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}