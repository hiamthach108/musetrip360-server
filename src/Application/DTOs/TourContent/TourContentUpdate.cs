using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class TourContentUpdateDto
{
    [MaxLength(1000)]
    public string? Content { get; set; }
    [Range(0, int.MaxValue)]
    public int? ZOrder { get; set; }
    public JsonDocument? Metadata { get; set; }
}

