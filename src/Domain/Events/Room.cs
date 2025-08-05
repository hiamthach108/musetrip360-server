using Application.Shared.Constant;
using Application.Shared.Enum;
using Application.Shared.Type;

public class Room : BaseEntity
{
    private static readonly Random _random = new Random();
    private string _id = null!;
    public new string Id
    {
        get => _id;
        set => _id = value;
    }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public RoomStatusEnum Status { get; set; }
    public Guid EventId { get; set; }
    public Room(Guid eventId, string name, RoomStatusEnum status, string? description)
    {
        Id = GenerateMeetCode();
        EventId = eventId;
        Name = name;
        Status = status;
        Description = description;
    }
    public Room()
    {
    }

    private string GenerateMeetCode()
    {
        string Part(int length) =>
            new string(Enumerable.Repeat(RoomConst.CHARS, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());

        return $"{Part(3)}-{Part(4)}-{Part(3)}";
    }
}
