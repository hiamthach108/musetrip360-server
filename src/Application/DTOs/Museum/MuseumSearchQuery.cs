namespace Application.DTOs.Museum;

using Application.Shared.Enum;

public class MuseumSearchQuery
{
  public string? Search { get; set; }
  public string? Location { get; set; }
  public MuseumStatusEnum? Status { get; set; }
  public double? MinRating { get; set; }
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 10;
  public string? SortBy { get; set; }
  public string? SortOrder { get; set; } = "asc";
}