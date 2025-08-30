using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class PayoutReq
{
    [Required]
    public Guid MuseumId { get; set; }
    [Required]
    [Range(1000, float.MaxValue, ErrorMessage = "Amount too small, must be greater than 1000đ")]
    public float Amount { get; set; }
    [Required]
    public Guid BankAccountId { get; set; }
    public JsonDocument? Metadata { get; set; }
}