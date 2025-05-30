using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class TourOnlineUpdateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(1000)]
    public string? Description { get; set; }
    public JsonDocument? Metadata { get; set; }
}

