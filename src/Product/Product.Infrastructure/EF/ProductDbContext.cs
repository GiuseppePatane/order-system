using Microsoft.EntityFrameworkCore;
using Product.Core.Domain;

namespace Product.Infrastructure.EF;

public class ProductDbContext : DbContext
{
    
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Category> Categories { get; set;  }
    public DbSet<Core.Domain.Product> Products { get; set;  }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
         modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}