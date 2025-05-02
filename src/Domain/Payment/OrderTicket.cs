namespace Domain.Payment;

using Domain.Tickets;

public class OrderTicket
{
  public Guid OrderId { get; set; }
  public Guid MasterId { get; set; }
  public Guid? AddonId { get; set; }
  public int Quantity { get; set; }
  public int GroupSize { get; set; }
  public float Price { get; set; }

  public Order Order { get; set; } = null!;
  public TicketMaster TicketMaster { get; set; } = null!;
  public TicketAddon? TicketAddon { get; set; }
}