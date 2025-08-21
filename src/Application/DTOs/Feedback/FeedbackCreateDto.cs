using System.ComponentModel.DataAnnotations;
using Application.Shared.Enum;

public class FeedbackCreateDto
{
    public DataEntityType Target { get; set; }
    public string Comment { get; set; } = null!;
    [Range(1, 5)]
    public int Rating { get; set; } = 5;
}