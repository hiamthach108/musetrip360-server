namespace Application.DTOs.Search;

public class IndexMessage
{
  public Guid Id { get; set; }
  public string Type { get; set; } = "";
  public string Action { get; set; } = "";
}