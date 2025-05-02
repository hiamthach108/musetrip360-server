namespace Domain.Artifacts;

using Application.Shared.Type;
using Domain.Events;
using Domain.Museums;
using Domain.Users;

public class Artifact : BaseEntity
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public string HistoricalPeriod { get; set; } = null!;
  public string ImageUrl { get; set; } = null!;
  public string Model3DUrl { get; set; } = null!;
  public float Rating { get; set; }
  public bool IsActive { get; set; }
  public Guid MuseumId { get; set; }
  public Guid CreatedBy { get; set; }

  public Museum Museum { get; set; } = null!;
  public User CreatedByUser { get; set; } = null!;

  public ICollection<Event> Events { get; set; } = null!;
}