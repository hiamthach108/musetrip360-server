using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class TourOnlineUpdateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(1000)]
    public string? Description { get; set; }
    [Range(0, float.MaxValue)]
    public float Price { get; set; }
    public JsonDocument? Metadata { get; set; }
}

