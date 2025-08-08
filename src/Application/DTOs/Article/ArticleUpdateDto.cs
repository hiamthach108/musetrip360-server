using System.ComponentModel.DataAnnotations;
using Application.Shared.Enum;
using Application.Shared.Type;

namespace Application.DTOs.Article;

public class ArticleUpdateDto
{
    [StringLength(200)]
    public string? Title { get; set; }
    
    public string? Content { get; set; }
    
    public Guid? MuseumId { get; set; }
    
    public ArticleStatusEnum? Status { get; set; }
    
    public DataEntityType? DataEntityType { get; set; }
    
    public Guid? EntityId { get; set; }
}