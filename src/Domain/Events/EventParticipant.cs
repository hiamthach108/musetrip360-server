namespace Domain.Events;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;

public class EventParticipant : BaseEntity
{
  public Guid EventId { get; set; }
  public Guid UserId { get; set; }
  public DateTime JoinedAt { get; set; }
  public ParticipantRoleEnum Role { get; set; }
  public ParticipantStatusEnum Status { get; set; }

  public Event Event { get; set; } = new();
  public User User { get; set; } = new();
}