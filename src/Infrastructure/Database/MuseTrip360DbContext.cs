namespace Database;

using Microsoft.EntityFrameworkCore;
using Domain.Users;
using Application.Shared.Constant;
using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Rolebase;
using Domain.Museums;
using Domain.Events;
using Domain.Artifacts;
using Domain.Payment;
using Domain.Tours;
using Domain.Messaging;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Domain.Subscription;
using Domain.Content;

public class MuseTrip360DbContext : DbContext
{
  // Users
  public DbSet<User> Users { get; set; }
  public DbSet<UserRole> UserRoles { get; set; }

  // Rolebase
  public DbSet<Role> Roles { get; set; }
  public DbSet<Permission> Permissions { get; set; }

  // Museums
  public DbSet<Museum> Museums { get; set; }
  public DbSet<MuseumRequest> MuseumRequests { get; set; }
  public DbSet<MuseumPolicy> MuseumPolicies { get; set; }
  public DbSet<Article> Articles { get; set; }

  // Event
  public DbSet<Event> Events { get; set; }
  public DbSet<EventParticipant> EventParticipants { get; set; }

  // Artifact 
  public DbSet<Artifact> Artifacts { get; set; }

  // Tours
  public DbSet<TourOnline> TourOnlines { get; set; }
  public DbSet<TourContent> TourContents { get; set; }
  public DbSet<TourGuide> TourGuides { get; set; }
  public DbSet<TourViewer> TourViewers { get; set; }

  // Payment
  public DbSet<Order> Orders { get; set; }
  public DbSet<Payment> Payments { get; set; }
  public DbSet<OrderEvent> OrderEvents { get; set; }
  public DbSet<OrderTour> OrderTours { get; set; }
  public DbSet<BankAccount> BankAccounts { get; set; }
  public DbSet<Payout> Payouts { get; set; }
  public DbSet<MuseumWallet> MuseumWallets { get; set; }
  public DbSet<Transaction> Transactions { get; set; }

  // Messaging
  public DbSet<Message> Messages { get; set; }
  public DbSet<Conversation> Conversations { get; set; }
  public DbSet<ConversationUser> ConversationUsers { get; set; }
  public DbSet<Notification> Notifications { get; set; }

  // Feedbacks
  public DbSet<Feedback> Feedbacks { get; set; }
  public DbSet<SystemReport> SystemReports { get; set; }

  // Subscription 
  public DbSet<Subscription> Subscriptions { get; set; }
  public DbSet<Plan> Plans { get; set; }

  // Content
  public DbSet<RepresentationMaterial> RepresentationMaterials { get; set; }
  public DbSet<Category> Categories { get; set; }
  public DbSet<HistoricalPeriod> HistoricalPeriods { get; set; }

  public MuseTrip360DbContext(DbContextOptions<MuseTrip360DbContext> options) : base(options) { }

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    var entries = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));
    foreach (var entry in entries)
    {
      if (entry.State == EntityState.Added)
      {
        ((BaseEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
      }
      ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
    }
    return base.SaveChangesAsync(cancellationToken);
  }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    foreach (var entityType in builder.Model.GetEntityTypes())
    {
      foreach (var property in entityType.GetProperties())
      {
        if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
        {
          property.SetValueConverter(
            new ValueConverter<DateTime, DateTime>(
              v => v.ToUniversalTime(),
              v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            ));
        }
      }
    }

    builder.Entity<User>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.Email).IsUnique();
      e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
      e.Property(x => x.Email).IsRequired().HasMaxLength(100);
      e.Property(x => x.PhoneNumber).IsRequired(false).HasMaxLength(20);
      e.Property(x => x.HashedPassword).IsRequired(false).HasMaxLength(100);
      e.Property(x => x.AvatarUrl).IsRequired(false).HasMaxLength(1000);
      e.Property(x => x.BirthDate).IsRequired(false);
      e.Property(x => x.AuthType).HasConversion<string>().HasDefaultValue(AuthTypeEnum.Email);
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(UserStatusEnum.NotVerified);
      e.Property(x => x.LastLogin).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<User>().HasData(new User
    {
      Id = Guid.Parse(GeneralConst.DEFAULT_GUID),
      FullName = "Admin",
      Username = "admin@admin.com",
      Email = "admin@admin.com",
      Status = UserStatusEnum.Active,
      AuthType = AuthTypeEnum.Email,
    });

    // Rolebase
    builder.Entity<Role>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.Name).IsUnique();
      e.Property(x => x.Name).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired(false);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

      e.HasMany(x => x.Permissions).WithMany(x => x.Roles);
    });

    builder.Entity<Permission>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.Name).IsUnique();
      e.Property(x => x.Name).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired(false);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

      e.HasMany(x => x.Roles).WithMany(x => x.Permissions);
    });

    builder.Entity<UserRole>(e =>
    {
      e.HasKey(x => new { x.UserId, x.RoleId, x.MuseumId });
      e.HasIndex(x => x.UserId);
      e.HasIndex(x => x.RoleId);
      e.HasIndex(x => x.MuseumId);

      e.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId);
      e.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId);
    });

    // Museums
    builder.Entity<Museum>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.Name).IsUnique();
      e.Property(x => x.Name).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired(false);
      e.Property(x => x.Location).IsRequired().HasMaxLength(100);
      e.Property(x => x.ContactEmail).IsRequired().HasMaxLength(100);
      e.Property(x => x.ContactPhone).IsRequired().HasMaxLength(20);
      e.Property(x => x.Latitude);
      e.Property(x => x.Longitude);
      e.Property(x => x.Rating).IsRequired().HasDefaultValue(0);
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(MuseumStatusEnum.Active);
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.Museums).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<MuseumPolicy>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.Property(x => x.Title).IsRequired().HasMaxLength(100);
      e.Property(x => x.Content).IsRequired().HasMaxLength(1000);
      e.Property(x => x.PolicyType).HasConversion<string>().HasDefaultValue(PolicyTypeEnum.TermsOfService);
      e.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedBy).IsRequired();
      e.HasOne(x => x.Museum).WithMany(x => x.MuseumPolicies).HasForeignKey(x => x.MuseumId);
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.MuseumPolicies).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<MuseumRequest>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.MuseumName).IsRequired().HasMaxLength(100);
      e.Property(x => x.MuseumDescription).IsRequired();
      e.Property(x => x.Location).IsRequired().HasMaxLength(100);
      e.Property(x => x.ContactEmail).IsRequired().HasMaxLength(100);
      e.Property(x => x.ContactPhone).IsRequired().HasMaxLength(20);
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(RequestStatusEnum.Pending);
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.MuseumRequests).HasForeignKey(x => x.CreatedBy);
      e.HasMany(x => x.Categories).WithMany();
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Article>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.Property(x => x.Title).IsRequired();
      e.Property(x => x.Content).IsRequired();
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedBy).IsRequired();
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(ArticleStatusEnum.Draft);
      e.Property(x => x.DataEntityType).HasConversion<string>().HasDefaultValue(DataEntityType.Museum);
      e.HasOne(x => x.Museum).WithMany(x => x.Articles).HasForeignKey(x => x.MuseumId);
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.Articles).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Event>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.Property(x => x.Title).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired();
      e.Property(x => x.EventType).HasConversion<string>().HasDefaultValue(EventTypeEnum.SpecialEvent);
      e.Property(x => x.StartTime).IsRequired();
      e.Property(x => x.EndTime).IsRequired();
      e.Property(x => x.Location).IsRequired().HasMaxLength(100);
      e.Property(x => x.Capacity).IsRequired();
      e.Property(x => x.AvailableSlots).IsRequired();
      e.Property(x => x.BookingDeadline).IsRequired();
      e.Property(x => x.Price).IsRequired().HasDefaultValue(0);
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(EventStatusEnum.Draft);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedBy).IsRequired();
      e.HasOne(x => x.Museum).WithMany(x => x.Events).HasForeignKey(x => x.MuseumId);
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.Events).HasForeignKey(x => x.CreatedBy);
      e.HasMany(x => x.Artifacts).WithMany(x => x.Events);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<EventParticipant>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.EventId);
      e.HasIndex(x => x.UserId);
      e.Property(x => x.JoinedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.Role).HasConversion<string>().HasDefaultValue(ParticipantRoleEnum.Attendee);
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(ParticipantStatusEnum.Pending);
      e.HasOne(x => x.Event).WithMany(x => x.EventParticipants).HasForeignKey(x => x.EventId);
      e.HasOne(x => x.User).WithMany(x => x.EventParticipants).HasForeignKey(x => x.UserId);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Artifact>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.Property(x => x.Name).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired();
      e.Property(x => x.HistoricalPeriod).IsRequired().HasMaxLength(100);
      e.Property(x => x.ImageUrl).IsRequired().HasMaxLength(1000);
      e.Property(x => x.Model3DUrl).IsRequired().HasMaxLength(1000);
      e.Property(x => x.Rating).IsRequired().HasDefaultValue(0);
      e.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedBy).IsRequired();
      e.HasOne(x => x.Museum).WithMany(x => x.Artifacts).HasForeignKey(x => x.MuseumId);
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.Artifacts).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<TourOnline>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.Property(x => x.Name).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired();
      e.Property(x => x.Price).IsRequired().HasDefaultValue(0);
      e.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.HasOne(x => x.Museum).WithMany(x => x.TourOnlines).HasForeignKey(x => x.MuseumId);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<TourContent>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.TourId);
      e.Property(x => x.Content).IsRequired().HasMaxLength(1000);
      e.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
      e.Property(x => x.ZOrder).IsRequired();
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.HasOne(x => x.TourOnline).WithMany(x => x.TourContents).HasForeignKey(x => x.TourId);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<TourGuide>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.Property(x => x.Name).IsRequired().HasMaxLength(100);
      e.Property(x => x.Bio).IsRequired().HasMaxLength(1000);
      e.Property(x => x.IsAvailable).IsRequired().HasDefaultValue(true);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.HasOne(x => x.Museum).WithMany(x => x.TourGuides).HasForeignKey(x => x.MuseumId);
      e.HasOne(x => x.User).WithMany(x => x.TourGuides).HasForeignKey(x => x.UserId);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<TourViewer>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.TourId);
      e.HasIndex(x => x.UserId);
      e.Property(x => x.AccessType).IsRequired().HasMaxLength(100);
      e.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
      e.Property(x => x.LastViewedAt).IsRequired(false);
      e.HasOne(x => x.TourOnline).WithMany(x => x.TourViewers).HasForeignKey(x => x.TourId);
      e.HasOne(x => x.User).WithMany(x => x.TourViewers).HasForeignKey(x => x.UserId);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Order>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.TotalAmount).IsRequired();
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(PaymentStatusEnum.Pending);
      e.Property(x => x.OrderType).HasConversion<string>().HasDefaultValue(OrderTypeEnum.Subscription);
      e.Property(x => x.OrderCode).IsRequired(false).HasMaxLength(100);
      e.Property(x => x.ExpiredAt).IsRequired();
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.Orders).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Payment>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.Amount).IsRequired();
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(PaymentStatusEnum.Pending);
      e.Property(x => x.PaymentMethod).HasConversion<string>().HasDefaultValue(PaymentMethodEnum.Cash);
      e.Property(x => x.TransactionId).IsRequired(false);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedBy).IsRequired();
      e.HasOne(x => x.Order).WithMany(x => x.Payments).HasForeignKey(x => x.OrderId);
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.Payments).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<OrderEvent>(e =>
    {
      e.HasKey(x => new { x.OrderId, x.EventId });
      e.HasIndex(x => x.OrderId);
      e.HasIndex(x => x.EventId);
      e.Property(x => x.UnitPrice).IsRequired();
      e.HasOne(x => x.Order).WithMany(x => x.OrderEvents).HasForeignKey(x => x.OrderId);
      e.HasOne(x => x.Event).WithMany(x => x.OrderEvents).HasForeignKey(x => x.EventId);
    });

    builder.Entity<OrderTour>(e =>
    {
      e.HasKey(x => new { x.OrderId, x.TourId });
      e.HasIndex(x => x.OrderId);
      e.HasIndex(x => x.TourId);
      e.Property(x => x.UnitPrice).IsRequired();
      e.HasOne(x => x.Order).WithMany(x => x.OrderTours).HasForeignKey(x => x.OrderId);
      e.HasOne(x => x.TourOnline).WithMany(x => x.OrderTours).HasForeignKey(x => x.TourId);
    });

    builder.Entity<Message>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.Content).IsRequired();
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.Messages).HasForeignKey(x => x.CreatedBy);
      e.HasOne(x => x.Conversation).WithMany().HasForeignKey(x => x.ConversationId);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Conversation>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.Name).IsRequired(false).HasMaxLength(100);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.IsBot).IsRequired().HasDefaultValue(false);
      e.Property(x => x.CreatedBy).IsRequired();
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.Conversations).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<ConversationUser>(e =>
    {
      e.HasKey(x => new { x.ConversationId, x.UserId });

      e.HasOne(x => x.Conversation).WithMany(x => x.ConversationUsers).HasForeignKey(x => x.ConversationId);
      e.HasOne(x => x.User).WithMany(x => x.ConversationUsers).HasForeignKey(x => x.UserId);
      e.HasOne(x => x.LastMessage).WithMany(x => x.ConversationUsers).HasForeignKey(x => x.LastMessageId);
      e.Property(x => x.LastMessageAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Notification>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.Title).IsRequired().HasMaxLength(100);
      e.Property(x => x.Message).IsRequired().HasMaxLength(1000);
      e.Property(x => x.Type).HasConversion<string>().HasDefaultValue(NotificationTargetEnum.User);
      e.Property(x => x.IsRead).IsRequired().HasDefaultValue(false);
      e.Property(x => x.ReadAt).IsRequired(false);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Feedback>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.Comment).IsRequired().HasMaxLength(1000);
      e.Property(x => x.Rating).IsRequired();
      e.Property(x => x.TargetId).IsRequired();
      e.Property(x => x.Type).HasConversion<string>().HasDefaultValue(DataEntityType.Museum);
      e.Property(x => x.CreatedBy).IsRequired();
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.Feedbacks).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<SystemReport>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.Title).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired();
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(ReportStatusEnum.Pending);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedBy).IsRequired();
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.SystemReports).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Plan>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.Name).IsUnique();
      e.Property(x => x.Name).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired(false);
      e.Property(x => x.Price).IsRequired();
      e.Property(x => x.DurationDays).IsRequired();
      e.Property(x => x.MaxEvents).IsRequired(false);
      e.Property(x => x.DiscountPercent).IsRequired(false);
      e.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Subscription>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasOne(x => x.User).WithMany(x => x.Subscriptions).HasForeignKey(x => x.UserId);
      e.HasOne(x => x.Plan).WithMany(x => x.Subscriptions).HasForeignKey(x => x.PlanId);
      e.HasOne(x => x.Order).WithMany(x => x.Subscriptions).HasForeignKey(x => x.OrderId);
      e.HasOne(x => x.Museum).WithMany(x => x.Subscriptions).HasForeignKey(x => x.MuseumId);
      e.Property(x => x.StartDate).IsRequired();
      e.Property(x => x.EndDate).IsRequired();
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(SubscriptionStatusEnum.Active);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<BankAccount>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.Property(x => x.HolderName).IsRequired().HasMaxLength(100);
      e.Property(x => x.BankName).IsRequired().HasMaxLength(100);
      e.Property(x => x.AccountNumber).IsRequired().HasMaxLength(100);
      e.Property(x => x.QRCode).IsRequired().HasMaxLength(1000);
      e.HasOne(x => x.Museum).WithMany(x => x.BankAccounts).HasForeignKey(x => x.MuseumId);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Payout>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.HasIndex(x => x.BankAccountId);
      e.Property(x => x.Amount).IsRequired();
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(PayoutStatusEnum.Pending);
      e.Property(x => x.ProcessedDate).IsRequired();
      e.HasOne(x => x.Museum).WithMany(x => x.Payouts).HasForeignKey(x => x.MuseumId);
      e.HasOne(x => x.BankAccount).WithMany(x => x.Payouts).HasForeignKey(x => x.BankAccountId);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<MuseumWallet>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.Property(x => x.AvailableBalance).IsRequired();
      e.Property(x => x.PendingBalance).IsRequired();
      e.Property(x => x.TotalBalance).IsRequired();
      e.HasOne(x => x.Museum).WithMany(x => x.MuseumWallets).HasForeignKey(x => x.MuseumId);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Transaction>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.MuseumId);
      e.Property(x => x.ReferenceId).IsRequired().HasMaxLength(100);
      e.Property(x => x.TransactionType).IsRequired().HasMaxLength(100);
      e.Property(x => x.Amount).IsRequired();
      e.Property(x => x.BalanceBefore).IsRequired();
      e.Property(x => x.BalanceAfter).IsRequired();
      e.HasOne(x => x.Museum).WithMany(x => x.Transactions).HasForeignKey(x => x.MuseumId);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<RepresentationMaterial>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.EventId);
      e.Property(x => x.Content).IsRequired().HasMaxLength(1000);
      e.Property(x => x.ZOrder).IsRequired();
      e.HasOne(x => x.Event).WithMany(x => x.RepresentationMaterials).HasForeignKey(x => x.EventId);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedBy).IsRequired();
      e.HasOne(x => x.CreatedByUser).WithMany(x => x.RepresentationMaterials).HasForeignKey(x => x.CreatedBy);
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });

    builder.Entity<Category>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.Name).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired(false).HasMaxLength(1000);
      e.HasMany(x => x.Museums).WithMany(x => x.Categories);
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });
    builder.Entity<HistoricalPeriod>(e =>
    {
      e.HasKey(x => x.Id);
      e.Property(x => x.Name).IsRequired().HasMaxLength(100);
      e.Property(x => x.Description).IsRequired(false).HasMaxLength(1000);
      e.Property(x => x.StartDate).IsRequired(false).HasMaxLength(100);
      e.Property(x => x.EndDate).IsRequired(false).HasMaxLength(100);
      e.Property(x => x.CreatedBy).IsRequired();
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
      e.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    });
  }

}