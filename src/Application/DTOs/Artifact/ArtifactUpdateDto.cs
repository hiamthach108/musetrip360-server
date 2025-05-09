using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class ArtifactUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }
    [Required]
    [MaxLength(1000)]
    public string? Description { get; set; }
    [Required]
    [MaxLength(100)]
    public string? HistoricalPeriod { get; set; }
    [Required]
    [MaxLength(1000)]
    public string? ImageUrl { get; set; }
    [Required]
    [MaxLength(1000)]
    public string? Model3DUrl { get; set; }
    public JsonDocument? Metadata { get; set; }
}
