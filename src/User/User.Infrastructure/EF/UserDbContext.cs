using Microsoft.EntityFrameworkCore;
using User.Core.Domain;

namespace User.Infrastructure.EF;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<UserEntity> Users { get; set;  } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
         modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
    
}