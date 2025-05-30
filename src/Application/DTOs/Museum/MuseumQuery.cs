namespace Application.DTOs.Museum;


using Application.DTOs.Pagination;
using Application.Shared.Enum;

public class MuseumQuery : PaginationReq
{
  public string? Search { get; set; }
  public MuseumStatusEnum? Status { get; set; }
}