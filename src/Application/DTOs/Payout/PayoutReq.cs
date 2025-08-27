public class PayoutReq
{
    public Guid MuseumId { get; set; }
    public Guid BankAccountId { get; set; }
    public float Amount { get; set; }
}