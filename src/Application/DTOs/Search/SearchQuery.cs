namespace Application.DTOs.Search;

using Application.DTOs.Pagination;
public class SearchQuery : PaginationReq
{
  public string? Search { get; set; }

  public string? Type { get; set; }

  public string? Location { get; set; }

  public double? RadiusKm { get; set; }
  public double? Latitude { get; set; }
  public double? Longitude { get; set; }

  public string? Status { get; set; }
}

