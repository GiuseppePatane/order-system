using Microsoft.EntityFrameworkCore;
using Address.Core.Domain;

namespace Address.Infrastructure.EF;

public class AddressDbContext : DbContext
{
    public AddressDbContext(DbContextOptions<AddressDbContext> options)
        : base(options)
    {
    }

    public DbSet<AddressEntity> Addresses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AddressDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
