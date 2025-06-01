namespace Application.DTOs.Museum;

using Application.Shared.Enum;

public class MuseumIndexDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Location { get; set; } = string.Empty;
  public string ContactEmail { get; set; } = string.Empty;
  public string ContactPhone { get; set; } = string.Empty;
  public double Rating { get; set; }
  public MuseumStatusEnum Status { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}