namespace Application.DTOs.Museum;

public class MuseumSuggestionDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Location { get; set; } = string.Empty;
  public double Rating { get; set; }
}