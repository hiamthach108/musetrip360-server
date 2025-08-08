using System.ComponentModel.DataAnnotations;
using Application.Shared.Enum;
using Application.Shared.Type;

namespace Application.DTOs.Article;

public class ArticleCreateDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;
    
    [Required]
    public string Content { get; set; } = null!;
    
    [Required]
    public Guid MuseumId { get; set; }
    
    public ArticleStatusEnum Status { get; set; } = ArticleStatusEnum.Draft;
    
    public DataEntityType DataEntityType { get; set; }
    
    public Guid? EntityId { get; set; }
}