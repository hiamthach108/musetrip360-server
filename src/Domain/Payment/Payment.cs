namespace Domain.Payment;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;

public class Payment : BaseEntity
{
  public Guid OrderId { get; set; }
  public float Amount { get; set; }
  public string? TransactionId { get; set; }
  public PaymentStatusEnum Status { get; set; }
  public PaymentMethodEnum PaymentMethod { get; set; }
  public Guid CreatedBy { get; set; }

  public Order Order { get; set; } = null!;
  public User CreatedByUser { get; set; } = null!;
}