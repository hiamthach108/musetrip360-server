using System.ComponentModel.DataAnnotations;
using Application.DTOs.Pagination;

public class EventQuery : PaginationReq, IValidatableObject
{
    [MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public string? EventType { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime.HasValue && EndTime.HasValue && StartTime > EndTime)
        {
            yield return new ValidationResult("Start time cannot be later than end time");
        }
    }
}
