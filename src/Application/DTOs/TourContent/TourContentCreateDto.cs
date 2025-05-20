using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class TourContentCreateDto
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = null!;
    [Required]
    [Range(0, int.MaxValue)]
    public int ZOrder { get; set; }
    public JsonDocument? Metadata { get; set; }
}
