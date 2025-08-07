namespace Domain.Content;

using Application.Shared.Type;

public class HistoricalPeriod : BaseEntity
{
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public string? StartDate { get; set; }
  public string? EndDate { get; set; }
  public Guid CreatedBy { get; set; }
}