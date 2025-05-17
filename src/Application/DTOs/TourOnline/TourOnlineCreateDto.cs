using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class TourOnlineCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = null!;
    public JsonDocument? Metadata { get; set; }
}
