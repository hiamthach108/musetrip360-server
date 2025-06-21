namespace Domain.Subscription;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Payment;
using Domain.Users;

public class Subscription : BaseEntity
{
  public Guid UserId { get; set; }
  public Guid PlanId { get; set; }
  public Guid OrderId { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  public SubscriptionStatusEnum Status { get; set; }

  public User User { get; set; } = new();
  public Plan Plan { get; set; } = new();
  public Order Order { get; set; } = new();
}