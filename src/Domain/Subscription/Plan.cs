namespace Domain.Subscription;

using Application.Shared.Type;

public class Plan : BaseEntity
{
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public decimal Price { get; set; }
  public int DurationDays { get; set; }
  public int? MaxEvents { get; set; }
  public decimal? DiscountPercent { get; set; }
  public bool IsActive { get; set; }

  public ICollection<Subscription> Subscriptions { get; set; } = null!;
}