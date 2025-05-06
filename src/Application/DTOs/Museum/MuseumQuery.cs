namespace Application.DTOs.Museum;


using Application.DTOs.Pagination;


public class MuseumQuery : PaginationReq
{
  public string? Search { get; set; }
}