namespace Domain.Payment;

using Domain.Events;

public class OrderEvent
{
  public Guid OrderId { get; set; }
  public Guid EventId { get; set; }
  public float UnitPrice { get; set; }

  public Order Order { get; set; } = new();
  public Event Event { get; set; } = new();
}