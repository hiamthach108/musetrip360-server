using Application.DTOs.Pagination;
using Application.Shared.Enum;

namespace Application.DTOs.Article;

public class ArticleQuery : PaginationReq
{
    public Guid? MuseumId { get; set; }
    public string? Search { get; set; }
}

public class ArticleAdminQuery : PaginationReq
{
    public Guid? MuseumId { get; set; }
    public string? Search { get; set; }
    public ArticleStatusEnum? Status { get; set; }
}