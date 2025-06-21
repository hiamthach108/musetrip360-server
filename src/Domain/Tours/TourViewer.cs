namespace Domain.Tours;

using Application.Shared.Type;
using Domain.Users;

public class TourViewer : BaseEntity
{
  public Guid UserId { get; set; }
  public Guid TourId { get; set; }
  public string AccessType { get; set; } = string.Empty;
  public bool IsActive { get; set; }
  public DateTime? LastViewedAt { get; set; }

  public User User { get; set; } = new();
  public TourOnline TourOnline { get; set; } = new();
}