using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class ArtifactUpdateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(1000)]
    public string? Description { get; set; }
    [MaxLength(100)]
    public string? HistoricalPeriod { get; set; }
    [MaxLength(1000)]
    public string? ImageUrl { get; set; }
    [MaxLength(1000)]
    public string? Model3DUrl { get; set; }
    public JsonDocument? Metadata { get; set; }
}
