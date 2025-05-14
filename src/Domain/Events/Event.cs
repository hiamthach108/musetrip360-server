namespace Domain.Events;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;
using Domain.Museums;
using Domain.Artifacts;
using Domain.Tours;
using Domain.Tickets;

public class Event : BaseEntity
{
  public string Title { get; set; } = null!;
  public string Description { get; set; } = null!;
  public EventTypeEnum EventType { get; set; }
  public DateTime StartTime { get; set; }
  public DateTime EndTime { get; set; }
  public string Location { get; set; } = null!;
  public int Capacity { get; set; }
  public int AvailableSlots { get; set; }
  public DateTime BookingDeadline { get; set; }
  public Guid MuseumId { get; set; }
  public Guid CreatedBy { get; set; }
  public EventStatusEnum Status { get; set; }

  public Museum Museum { get; set; } = null!;
  public User CreatedByUser { get; set; } = null!;

  public ICollection<Artifact> Artifacts { get; set; } = null!;
  public ICollection<TourOnline> TourOnlines { get; set; } = null!;
  public ICollection<TourGuide> TourGuides { get; set; } = null!;
  public ICollection<TicketAddon> TicketAddons { get; set; } = null!;
}