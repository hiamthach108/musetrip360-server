namespace Application.DTOs.Museum;

public class MuseumAggregationsDto
{
  public int TotalMuseums { get; set; }
  public Dictionary<string, int> MuseumsByStatus { get; set; } = new();
  public Dictionary<string, int> MuseumsByLocation { get; set; } = new();
  public double AverageRating { get; set; }
}