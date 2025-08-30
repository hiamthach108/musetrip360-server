using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class PayoutReq
{
    [Required]
    public Guid MuseumId { get; set; }
    [Required]
    public Guid BankAccountId { get; set; }
    [Required]
    [Range(0, float.MaxValue)]
    public float Amount { get; set; }
    public JsonDocument? Metadata { get; set; }
}