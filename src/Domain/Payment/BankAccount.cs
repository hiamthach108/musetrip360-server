namespace Domain.Payment;

using Application.Shared.Type;
using Domain.Museums;

public class BankAccount : BaseEntity
{
  public Guid MuseumId { get; set; }
  public string HolderName { get; set; } = null!;
  public string BankName { get; set; } = null!;
  public string AccountNumber { get; set; } = null!;
  public string QRCode { get; set; } = null!;

  public Museum Museum { get; set; } = null!;
  public ICollection<Payout> Payouts { get; set; } = [];
}