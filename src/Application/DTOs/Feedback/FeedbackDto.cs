using System.ComponentModel.DataAnnotations;

public class FeedbackCreateDto
{
    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = null!;
}