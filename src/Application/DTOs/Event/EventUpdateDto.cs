using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Shared.Enum;

public class EventUpdateDto : IValidatableObject
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
    [Range(0, int.MaxValue)]
    public int? AvailableSlots { get; set; }
    public DateTime? BookingDeadline { get; set; }
    [Range(0, float.MaxValue)]
    public float? Price { get; set; }
    public JsonDocument? Metadata { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate AvailableSlots against Capacity
        if (AvailableSlots.HasValue && Capacity.HasValue && AvailableSlots.Value > Capacity.Value)
        {
            yield return new ValidationResult("Available slots cannot be greater than capacity");
        }
    }
}
