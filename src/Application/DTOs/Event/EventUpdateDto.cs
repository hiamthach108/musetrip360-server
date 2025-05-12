using System.ComponentModel.DataAnnotations;
using Application.Shared.Enum;

public class EventUpdateDto
{
    [MaxLength(100)]
    public string? Title { get; set; }
    [MaxLength(1000)]
    public string? Description { get; set; }
    public EventTypeEnum? EventType { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    [MaxLength(100)]
    public string? Location { get; set; }
    public int? Capacity { get; set; }
    public int? AvailableSlots { get; set; }
    public DateTime? BookingDeadline { get; set; }
}