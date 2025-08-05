using Application.Shared.Enum;

public class RoomUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public RoomStatusEnum? Status { get; set; }
}