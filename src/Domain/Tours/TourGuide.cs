namespace Domain.Tours;

using Application.Shared.Type;
using Domain.Events;
using Domain.Museums;
using Domain.Users;

public class TourGuide : BaseEntity
{
  public string Name { get; set; } = null!;
  public string Bio { get; set; } = null!;
  public bool IsAvailable { get; set; }
  public Guid MuseumId { get; set; }
  public Guid UserId { get; set; }
  public Museum Museum { get; set; } = null!;
  public User User { get; set; } = null!;

  public ICollection<Event> Events { get; set; } = new List<Event>();
}