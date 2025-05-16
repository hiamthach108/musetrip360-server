using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Shared.Enum;

public class EventUpdateDto : IValidatableObject
{
    [MaxLength(100)]
    public string? Title { get; set; }
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = null!;
    public EventTypeEnum? EventType { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
    [Required]
    [MaxLength(100)]
    public string Location { get; set; } = null!;
    [Required]
    [Range(0, int.MaxValue)]
    public int Capacity { get; set; }
    [Required]
    [Range(0, int.MaxValue)]
    public int AvailableSlots { get; set; }
    [Required]
    public DateTime BookingDeadline { get; set; }
    public JsonDocument? Metadata { get; set; }

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
        if (StartTime >= EndTime)
        {
            yield return new ValidationResult("Start time must be before end time");
        }
        if (AvailableSlots > Capacity)
        {
            yield return new ValidationResult("Available slots cannot be greater than capacity");
        }
        if (BookingDeadline <= StartTime || BookingDeadline >= EndTime)
        {
            yield return new ValidationResult("Booking deadline must be between start and end time");
        }
    }
}
