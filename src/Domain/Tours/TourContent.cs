namespace Domain.Tours;

using Application.Shared.Type;

public class TourContent : BaseEntity
{
  public Guid TourId { get; set; }
  public string Content { get; set; } = null!;
  public bool IsActive { get; set; }
  public int ZOrder { get; set; }

  public TourOnline TourOnline { get; set; } = null!;
}