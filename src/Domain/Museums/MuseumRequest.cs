namespace Domain.Museums;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Content;
using Domain.Users;

public class MuseumRequest : BaseEntity
{
  public string MuseumName { get; set; } = null!;
  public string MuseumDescription { get; set; } = null!;
  public string Location { get; set; } = null!;
  public string ContactEmail { get; set; } = null!;
  public string ContactPhone { get; set; } = null!;
  public DateTime SubmittedAt { get; set; }
  public RequestStatusEnum Status { get; set; }
  public Guid CreatedBy { get; set; }

  public User CreatedByUser { get; set; } = null!;
  public ICollection<Category> Categories { get; set; } = new List<Category>();
}