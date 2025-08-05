using System.ComponentModel.DataAnnotations;
using Application.Shared.Enum;

public class RoomCreateDto
{
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public RoomStatusEnum Status { get; set; } = RoomStatusEnum.Inactive;
}