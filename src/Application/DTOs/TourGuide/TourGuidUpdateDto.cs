using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class TourGuideUpdateDto
{
  [MaxLength(100)]
  public string? Name { get; set; } = null!;
  [MaxLength(1000)]
  public string? Bio { get; set; } = null!;
  public JsonDocument? Metadata { get; set; }
}