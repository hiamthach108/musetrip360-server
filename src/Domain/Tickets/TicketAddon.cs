namespace Domain.Tickets;

using Application.Shared.Type;
using Domain.Events;
using Domain.Museums;
using Domain.Payment;

public class TicketAddon : BaseEntity
{
  public Guid MuseumId { get; set; }
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public float Price { get; set; }
  public bool IsActive { get; set; }
  public Guid EventId { get; set; }

  public Museum Museum { get; set; } = null!;
  public Event Event { get; set; } = null!;

  public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
  public ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();

}