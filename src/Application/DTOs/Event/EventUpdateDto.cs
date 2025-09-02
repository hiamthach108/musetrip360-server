using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Shared.Enum;

public class EventUpdateDto
{
    [MaxLength(100)]
    public string? Title { get; set; }
    [MaxLength(1000)]
    public string? Description { get; set; }
    public EventTypeEnum? EventType { get; set; }
    [MaxLength(100)]
    public string? Location { get; set; }
    [Range(0, int.MaxValue)]
    public int? Capacity { get; set; }
    public DateTime? BookingDeadline { get; set; }
    [Range(0, float.MaxValue)]
    public float? Price { get; set; }
    public JsonDocument? Metadata { get; set; }
}
