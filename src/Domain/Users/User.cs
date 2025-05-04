namespace Domain.Users;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Artifacts;
using Domain.Events;
using Domain.Messaging;
using Domain.Museums;
using Domain.Payment;
using Domain.Reviews;
using Domain.Tickets;
using Domain.Tours;

public class User : BaseEntity
{
  public string Username { get; set; } = null!;
  public string FullName { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string? PhoneNumber { get; set; }
  public string? HashedPassword { get; set; }
  public string? AvatarUrl { get; set; }
  public DateTime? BirthDate { get; set; }
  public AuthTypeEnum AuthType { get; set; }
  public UserStatusEnum Status { get; set; }
  public DateTime LastLogin { get; set; }

  public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
  public ICollection<MuseumPolicy> MuseumPolicies { get; set; } = new List<MuseumPolicy>();
  public ICollection<Article> Articles { get; set; } = new List<Article>();
  public ICollection<Event> Events { get; set; } = new List<Event>();
  public ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();
  public ICollection<TicketMaster> TicketMasters { get; set; } = new List<TicketMaster>();
  public ICollection<TourOnline> TourOnlines { get; set; } = new List<TourOnline>();
  public ICollection<TourGuide> TourGuides { get; set; } = new List<TourGuide>();
  public ICollection<Museum> Museums { get; set; } = new List<Museum>();
  public ICollection<MuseumRequest> MuseumRequests { get; set; } = new List<MuseumRequest>();
  public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
  public ICollection<Order> Orders { get; set; } = new List<Order>();
  public ICollection<Payment> Payments { get; set; } = new List<Payment>();
  public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
  public ICollection<Message> Messages { get; set; } = new List<Message>();
  public ICollection<ConversationUser> ConversationUsers { get; set; } = new List<ConversationUser>();
  public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

  public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
  public ICollection<SystemReport> SystemReports { get; set; } = new List<SystemReport>();
}