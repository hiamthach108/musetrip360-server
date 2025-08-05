using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class ArtifactCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    [MaxLength(100)]
    public string HistoricalPeriod { get; set; } = null!;
    [MaxLength(1000)]
    public string ImageUrl { get; set; } = null!;
    public string? Model3DUrl { get; set; } = null!;
    public JsonDocument? Metadata { get; set; }
}

