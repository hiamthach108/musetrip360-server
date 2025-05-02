namespace Domain.Museums;

using Application.Shared.Enum;
using Application.Shared.Type;

public class MuseumRequest : BaseEntity
{
  public string MuseumName { get; set; } = null!;
  public string MuseumDescription { get; set; } = null!;
  public string Location { get; set; } = null!;
  public string ContactEmail { get; set; } = null!;
  public string ContactPhone { get; set; } = null!;
  public DateTime SubmittedAt { get; set; }
  public RequestStatusEnum Status { get; set; }
}