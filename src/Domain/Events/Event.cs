namespace Domain.Events;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;
using Domain.Museums;
using Domain.Artifacts;
using Domain.Tours;
using Domain.Content;
using Domain.Payment;

public class Event : BaseEntity
{
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public EventTypeEnum EventType { get; set; }
  public DateTime StartTime { get; set; }
  public DateTime EndTime { get; set; }
  public string Location { get; set; } = string.Empty;
  public int Capacity { get; set; }
  public int AvailableSlots { get; set; }
  public DateTime BookingDeadline { get; set; }
  public Guid MuseumId { get; set; }
  public Guid CreatedBy { get; set; }
  public EventStatusEnum Status { get; set; }

  public Museum Museum { get; set; } = new();
  public User CreatedByUser { get; set; } = new();

  public ICollection<Artifact> Artifacts { get; set; } = [];
  public ICollection<TourOnline> TourOnlines { get; set; } = [];
  public ICollection<TourGuide> TourGuides { get; set; } = [];
  public ICollection<EventParticipant> EventParticipants { get; set; } = [];
  public ICollection<RepresentationMaterial> RepresentationMaterials { get; set; } = [];
  public ICollection<OrderEvent> OrderEvents { get; set; } = [];
}