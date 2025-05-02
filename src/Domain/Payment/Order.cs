namespace Domain.Payment;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;

public class Order : BaseEntity
{
  public Guid CreatedBy { get; set; }
  public float TotalAmount { get; set; }
  public PaymentStatusEnum Status { get; set; }

  public User CreatedByUser { get; set; } = null!;
  public ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();
  public ICollection<Payment> Payments { get; set; } = new List<Payment>();

}
