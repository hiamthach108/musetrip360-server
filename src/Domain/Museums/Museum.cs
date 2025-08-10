namespace Domain.Museums;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Artifacts;
using Domain.Content;
using Domain.Events;
using Domain.Payment;
using Domain.Tours;
using Domain.Users;

public class Museum : BaseEntity
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public string Location { get; set; } = null!;
  public string ContactEmail { get; set; } = null!;
  public string ContactPhone { get; set; } = null!;
  public decimal Latitude { get; set; }
  public decimal Longitude { get; set; }
  public float Rating { get; set; }
  public Guid CreatedBy { get; set; }
  public MuseumStatusEnum Status { get; set; }

  public User CreatedByUser { get; set; } = null!;

  public ICollection<MuseumPolicy> MuseumPolicies { get; set; } = new List<MuseumPolicy>();
  public ICollection<Article> Articles { get; set; } = new List<Article>();
  public ICollection<Event> Events { get; set; } = new List<Event>();
  public ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();
  public ICollection<TourOnline> TourOnlines { get; set; } = new List<TourOnline>();
  public ICollection<TourGuide> TourGuides { get; set; } = new List<TourGuide>();
  public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
  public ICollection<Payout> Payouts { get; set; } = new List<Payout>();
  public ICollection<MuseumWallet> MuseumWallets { get; set; } = new List<MuseumWallet>();

  public ICollection<Category> Categories { get; set; } = new List<Category>();
  public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
