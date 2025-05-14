using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Shared.Enum;

public class EventCreateDto : IValidatableObject
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = null!;
    [MaxLength(1000)]
    public string Description { get; set; } = null!;
    public EventTypeEnum EventType { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
    [Required]
    [MaxLength(100)]
    public string Location { get; set; } = null!;
    [Required]
    public int Capacity { get; set; }
    [Required]
    public int AvailableSlots { get; set; }
    [Required]
    public DateTime BookingDeadline { get; set; }
    public JsonDocument? Metadata { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime >= EndTime)
        {
            yield return new ValidationResult("Start time must be before end time");
        }
        if (BookingDeadline <= DateTime.Now)
        {
            yield return new ValidationResult("Booking deadline must be in the future");
        }
    }
}
