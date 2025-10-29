using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Address.Infrastructure.EF;

/// <summary>
/// Design-time factory for creating AddressDbContext instances for EF Core migrations
/// </summary>
public class AddressDbContextFactory : IDesignTimeDbContextFactory<AddressDbContext>
{
    public AddressDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AddressDbContext>();

        // Use a dummy connection string for migrations - actual connection will be configured at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=addressdb;Username=postgres;Password=postgres");

        return new AddressDbContext(optionsBuilder.Options);
    }
}
