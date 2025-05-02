namespace Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class MuseTrip360DbContextFactory : IDesignTimeDbContextFactory<MuseTrip360DbContext>
{
  public MuseTrip360DbContext CreateDbContext(string[] args)
  {
    var configuration = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json")
      .Build();

    Console.WriteLine($"Using ConnectionString: {configuration.GetConnectionString("DatabaseConnection")}");

    var optionsBuilder = new DbContextOptionsBuilder<MuseTrip360DbContext>();
    optionsBuilder.UseNpgsql(
      configuration.GetConnectionString("DatabaseConnection") ?? ""
    );

    return new MuseTrip360DbContext(optionsBuilder.Options);
  }
}
