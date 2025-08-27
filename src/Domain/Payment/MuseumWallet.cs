namespace Domain.Payment;

using Application.Shared.Type;
using Domain.Museums;

public class MuseumWallet : BaseEntity
{
  public Guid MuseumId { get; set; }
  public float AvailableBalance { get; set; }
  public float PendingBalance { get; set; }
  public float TotalBalance { get; set; }

  public Museum Museum { get; set; } = null!;
  public void AddBalance(float amount)
  {
    AvailableBalance += amount;
    TotalBalance += amount;
  }
  public void HoldBalance(float amount)
  {
    AvailableBalance -= amount;
    PendingBalance += amount;
  }
  public void WithdrawBalance()
  {
    TotalBalance -= PendingBalance;
    PendingBalance = 0;
  }

  public void RejectPayout()
  {
    AvailableBalance += PendingBalance;
    PendingBalance = 0;
  }
}
