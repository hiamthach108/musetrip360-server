using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class TourGuideUpdateDto
{
  [Required]
  [MaxLength(100)]
  public string Name { get; set; } = null!;
  [Required]
  [MaxLength(1000)]
  public string Bio { get; set; } = null!;
  public JsonDocument? Metadata { get; set; }
}