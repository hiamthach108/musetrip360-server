namespace Domain.Tickets;

using Application.Shared.Type;
using Domain.Museums;
using Domain.Payment;

public class TicketMaster : BaseEntity
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public float Price { get; set; }
  public float DiscountPercentage { get; set; }
  public int GroupSize { get; set; }
  public bool IsActive { get; set; }
  public Guid MuseumId { get; set; }

  public Museum Museum { get; set; } = null!;

  public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
  public ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();
}