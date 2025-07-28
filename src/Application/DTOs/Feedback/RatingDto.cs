
using System.ComponentModel.DataAnnotations;

public class RatingCreateDto
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = null!;
}