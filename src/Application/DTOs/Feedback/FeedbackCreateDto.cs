using System.ComponentModel.DataAnnotations;
using Application.DTOs.Pagination;
using Application.Shared.Enum;

public class FeedbackCreateDto
{
    public Guid TargetId { get; set; }
    public DataEntityType Target { get; set; }
    public string Comment { get; set; } = null!;
    [Range(1, 5)]
    public int Rating { get; set; } = 5;
}

public class FeedbackQuery : PaginationReq
{
    public Guid TargetId { get; set; }
    public DataEntityType Type { get; set; }
}