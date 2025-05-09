using System.Text.Json;

public class ArtifactUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? HistoricalPeriod { get; set; }
    public string? ImageUrl { get; set; }
    public string? Model3DUrl { get; set; }
    public JsonDocument? Metadata { get; set; }
}
