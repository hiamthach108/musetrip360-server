using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class ArtifactCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = null!;
    [Required]
    [MaxLength(100)]
    public string HistoricalPeriod { get; set; } = null!;
    [Required]
    [MaxLength(1000)]
    public string ImageUrl { get; set; } = null!;
    [Required]
    [MaxLength(1000)]
    public string Model3DUrl { get; set; } = null!;
    public JsonDocument? Metadata { get; set; }
}

