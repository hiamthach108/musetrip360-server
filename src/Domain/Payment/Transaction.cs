namespace Domain.Payment;

using Application.Shared.Type;
using Domain.Museums;

public class Transaction : BaseEntity
{
  public Guid MuseumId { get; set; }
  public string ReferenceId { get; set; } = null!;
  public string TransactionType { get; set; } = null!; // e.g., "Payout", "Order", "Refund"
  public decimal Amount { get; set; }
  public decimal BalanceBefore { get; set; }
  public decimal BalanceAfter { get; set; }

  public Museum Museum { get; set; } = null!;
}