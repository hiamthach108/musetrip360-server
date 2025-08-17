namespace Application.DTOs.Museum;


using Application.DTOs.Pagination;
using Application.Shared.Enum;

public class MuseumQuery : PaginationReq
{
  public string? Search { get; set; }
  public List<MuseumStatusEnum>? Status { get; set; }
  public Guid? CategoryId { get; set; }
}