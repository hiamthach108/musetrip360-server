namespace Domain.Payment;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Museums;

public class Payout : BaseEntity
{
  public Guid MuseumId { get; set; }
  public Guid BankAccountId { get; set; }
  public float Amount { get; set; }
  public DateTime ProcessedDate { get; set; }
  public PayoutStatusEnum Status { get; set; }

  public Museum Museum { get; set; } = null!;
  public BankAccount BankAccount { get; set; } = null!;
}