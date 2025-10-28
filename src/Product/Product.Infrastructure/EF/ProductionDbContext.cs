using Microsoft.EntityFrameworkCore;
using Product.Core.Domain;

namespace Product.Infrastructure.EF;

public class ProductionDbContext : DbContext
{
    
    public ProductionDbContext(DbContextOptions<ProductionDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Category> Categories { get; set;  }
    public DbSet<Core.Domain.Product> Products { get; set;  }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
         modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductionDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}