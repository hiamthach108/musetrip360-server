namespace Database;

using Microsoft.EntityFrameworkCore;
using Domain.Users;
using Application.Shared.Constant;
using Application.Shared.Enum;
using Application.Shared.Type;
public class MuseTrip360DbContext : DbContext
{
  // Users
  public DbSet<User> Users { get; set; }

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


    builder.Entity<User>(e =>
    {
      e.HasKey(x => x.Id);
      e.HasIndex(x => x.Email).IsUnique();
      e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
      e.Property(x => x.Email).IsRequired().HasMaxLength(100);
      e.Property(x => x.Phone).IsRequired(false).HasMaxLength(20);
      e.Property(x => x.HashedPassword).IsRequired(false).HasMaxLength(100);
      e.Property(x => x.Avatar).IsRequired(false).HasMaxLength(1000);
      e.Property(x => x.AuthType).HasConversion<string>().HasDefaultValue(AuthTypeEnum.Email);
      e.Property(x => x.Status).HasConversion<string>().HasDefaultValue(UserStatusEnum.NotVerified);
      e.Property(x => x.LastLogin).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
      e.Property(x => x.Metadata).IsRequired(false).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb"); ;
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
    }, new User
    {
      Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
      FullName = "User1",
      Username = "user1@user.com",
      Email = "user1@user.com",
      Status = UserStatusEnum.Active,
      AuthType = AuthTypeEnum.Email,
    });
  }

}