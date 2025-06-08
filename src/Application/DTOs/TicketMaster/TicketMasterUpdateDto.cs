using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class TicketMasterUpdateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(1000)]
    public string? Description { get; set; }
    [Range(0, float.MaxValue)]
    public float? Price { get; set; }
    [Range(0, 100)]
    public float? DiscountPercentage { get; set; }
    [Range(1, int.MaxValue)]
    public int? GroupSize { get; set; }
    public JsonDocument? Metadata { get; set; }
}