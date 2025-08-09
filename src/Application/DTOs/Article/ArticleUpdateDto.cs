namespace Application.DTOs.Article;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Shared.Enum;


public class ArticleUpdateDto
{
    [StringLength(200)]
    public string? Title { get; set; }

    public string? Content { get; set; }

    public Guid? MuseumId { get; set; }

    public ArticleStatusEnum? Status { get; set; }

    public DataEntityType? DataEntityType { get; set; }

    public Guid? EntityId { get; set; }
    public JsonDocument? Metadata { get; set; }
}