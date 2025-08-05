using System.Text.Json;
using StackExchange.Redis;

public interface IRoomRepository
{
    Task<Room?> GetRoomById(string id);
    Task CreateRoom(Room room);
    Task UpdateRoom(string id, Room room);
    Task DeleteRoom(string id);
    Task<IEnumerable<Room>> GetRoomByEventId(Guid id);
}

public class RoomRepository : IRoomRepository
{
    private readonly IDatabase _database;

    public RoomRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task CreateRoom(Room room)
    {
        await _database.StringSetAsync($"room:{room.Id}", JsonSerializer.Serialize(room));
        await _database.SetAddAsync($"room:event:{room.EventId}", room.Id);
    }

    public async Task DeleteRoom(string id)
    {
        await _database.KeyDeleteAsync($"room:{id}");
    }

    public async Task<Room?> GetRoomById(string id)
    {
        var roomJson = await _database.StringGetAsync($"room:{id}");
        return roomJson.HasValue ? JsonSerializer.Deserialize<Room>(roomJson!) : null;
    }

    public async Task<IEnumerable<Room>> GetRoomByEventId(Guid id)
    {
        var roomIds = await _database.SetMembersAsync($"room:event:{id}");
        var roomKeys = roomIds.Select(id => (RedisKey)$"room:{id}").ToArray();
        var rooms = await _database.StringGetAsync(roomKeys);
        return rooms.Where(room => room.HasValue).Select(room => JsonSerializer.Deserialize<Room>(room!)).Where(room => room != null)!;
    }

    public async Task UpdateRoom(string id, Room room)
    {
        await _database.StringSetAsync($"room:{id}", JsonSerializer.Serialize(room));
    }
}