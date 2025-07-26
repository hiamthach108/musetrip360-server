namespace Domain.Tours;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Events;
using Domain.Museums;
using Domain.Payment;

public class TourOnline : BaseEntity
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public bool IsActive { get; set; }
  public TourStatusEnum Status { get; set; }
  public Guid MuseumId { get; set; }

  public Museum Museum { get; set; } = null!;

  public ICollection<TourContent> TourContents { get; set; } = new List<TourContent>();
  public ICollection<Event> Events { get; set; } = new List<Event>();
  public ICollection<OrderTour> OrderTours { get; set; } = [];
  public ICollection<TourViewer> TourViewers { get; set; } = [];
}