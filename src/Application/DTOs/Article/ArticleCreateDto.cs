namespace Application.DTOs.Article;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Shared.Enum;


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
    public JsonDocument? Metadata { get; set; }
}