using System.ComponentModel.DataAnnotations;
using Application.DTOs.Pagination;
using Application.Shared.Enum;

public class EventAdminQuery : EventQuery {
    public DateTime? StartBookingDeadline { get; set; }
    public DateTime? EndBookingDeadline { get; set; }
    public EventStatusEnum? Status { get; set; }
    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartBookingDeadline.HasValue && EndBookingDeadline.HasValue && StartBookingDeadline > EndBookingDeadline)
        {
            yield return new ValidationResult("Start booking deadline must be before end booking deadline");
        }
    }
    
}
