namespace Domain.Payment;

using Domain.Tours;

public class OrderTour
{
  public Guid OrderId { get; set; }
  public Guid TourId { get; set; }
  public float UnitPrice { get; set; }

  public Order Order { get; set; } = new();
  public TourOnline TourOnline { get; set; } = new();
}