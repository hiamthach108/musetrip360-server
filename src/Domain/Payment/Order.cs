namespace Domain.Payment;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Subscription;
using Domain.Users;

public class Order : BaseEntity
{
  public Guid CreatedBy { get; set; }
  public float TotalAmount { get; set; }
  public string OrderCode { get; set; } = null!;
  public DateTime ExpiredAt { get; set; }
  public PaymentStatusEnum Status { get; set; }
  public OrderTypeEnum OrderType { get; set; }

  public User CreatedByUser { get; set; } = null!;

  public ICollection<Payment> Payments { get; set; } = [];
  public ICollection<Subscription> Subscriptions { get; set; } = [];
  public ICollection<OrderEvent> OrderEvents { get; set; } = [];
  public ICollection<OrderTour> OrderTours { get; set; } = [];
}
