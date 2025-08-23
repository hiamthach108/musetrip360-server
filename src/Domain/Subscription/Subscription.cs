namespace Domain.Subscription;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Payment;
using Domain.Users;
using Domain.Museums;

public class Subscription : BaseEntity
{
  public Guid UserId { get; set; }
  public Guid PlanId { get; set; }
  public Guid OrderId { get; set; }
  public Guid MuseumId { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  public SubscriptionStatusEnum Status { get; set; }

  public User User { get; set; } = null!;
  public Plan Plan { get; set; } = null!;
  public Order Order { get; set; } = null!;
  public Museum Museum { get; set; } = null!;
}