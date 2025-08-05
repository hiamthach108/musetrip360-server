using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class ArtifactUpdateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    public string? Description { get; set; }
    [MaxLength(100)]
    public string? HistoricalPeriod { get; set; }
    public string? ImageUrl { get; set; }
    public string? Model3DUrl { get; set; }
    public JsonDocument? Metadata { get; set; }
}
