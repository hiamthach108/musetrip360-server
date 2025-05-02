namespace Domain.Tickets;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;

public class Ticket : BaseEntity
{
  public Guid MasterId { get; set; }
  public DateTime PurchaseDate { get; set; }
  public int GroupSize { get; set; }
  public float TotalPrice { get; set; }
  public string QRCode { get; set; } = null!;
  public DateTime? ExpiredTime { get; set; }
  public TicketStatusEnum Status { get; set; }
  public Guid OwnerId { get; set; }

  public TicketMaster TicketMaster { get; set; } = null!;
  public User Owner { get; set; } = null!;

  public ICollection<TicketAddon> TicketAddons { get; set; } = new List<TicketAddon>();
}