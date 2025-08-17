using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Application.DTOs.Pagination;
using Application.Shared.Enum;

public class EventQuery : PaginationReq, IValidatableObject
{
    public Guid? MuseumId { get; set; }
    [MaxLength(1000)]
    public string? Search { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }
    public EventTypeEnum? EventType { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<Guid>? Ids { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime.HasValue && EndTime.HasValue && StartTime > EndTime)
        {
            yield return new ValidationResult("Start time cannot be later than end time");
        }
    }
}
