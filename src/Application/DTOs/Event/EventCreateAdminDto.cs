using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Shared.Enum;

public class EventCreateAdminDto : IValidatableObject
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = null!;
    [MaxLength(1000)]
    public string? Description { get; set; }
    [Required]
    public EventTypeEnum EventType { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    [MaxLength(100)]
    public string? Location { get; set; }
    [Range(0, int.MaxValue)]
    public int? Capacity { get; set; }
    [Range(0, int.MaxValue)]
    public int? AvailableSlots { get; set; }
    public DateTime? BookingDeadline { get; set; }
    public JsonDocument? Metadata { get; set; }
    [Required]
    public EventStatusEnum Status { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime <= DateTime.Now || EndTime <= DateTime.Now)
        {
            yield return new ValidationResult("Start and end time must be in the future");
        }
        if (BookingDeadline <= DateTime.Now)
        {
            yield return new ValidationResult("Booking deadline must be in the future");
        }
        if (StartTime.HasValue && EndTime.HasValue && StartTime.Value >= EndTime.Value)
        {
            yield return new ValidationResult("Start time must be before end time");
        }
    }
}