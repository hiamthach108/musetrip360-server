using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class TourGuideCreateDto
{
  [Required]
  [MaxLength(100)]
  public string Name { get; set; } = null!;
  [Required]
  [MaxLength(1000)]
  public string Bio { get; set; } = null!;
  [Required]
  public Guid UserId { get; set; }
  public JsonDocument? Metadata { get; set; }
}