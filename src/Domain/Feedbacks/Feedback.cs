namespace Domain.Reviews;

using Application.Shared.Type;
using Domain.Museums;
using Domain.Users;

public class Feedback : BaseEntity
{
  public string Comment { get; set; } = null!;
  public int Rating { get; set; }
  public Guid MuseumId { get; set; }
  public Guid CreatedBy { get; set; }

  public Museum Museum { get; set; } = null!;
  public User CreatedByUser { get; set; } = null!;
}