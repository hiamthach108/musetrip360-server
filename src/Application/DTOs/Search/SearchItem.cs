namespace Application.DTOs.Search;

public class SearchItem
{
  public Guid Id { get; set; }
  public string Title { get; set; } = "";
  public string Type { get; set; } = "";
  public string? Thumbnail { get; set; }
  public string? Description { get; set; }
  public decimal? Latitude { get; set; }
  public decimal? Longitude { get; set; }
  public string? Location { get; set; }
}