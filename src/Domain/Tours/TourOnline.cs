namespace Domain.Tours;

using Application.Shared.Type;
using Domain.Events;
using Domain.Museums;

public class TourOnline : BaseEntity
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public bool IsActive { get; set; }
  public Guid MuseumId { get; set; }

  public Museum Museum { get; set; } = null!;

  public ICollection<TourContent> TourContents { get; set; } = new List<TourContent>();
  public ICollection<Event> Events { get; set; } = new List<Event>();
}