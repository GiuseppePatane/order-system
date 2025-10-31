using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Address.Core.Domain;

namespace Address.Infrastructure.EF.Configuration;

public class AddressConfiguration : IEntityTypeConfiguration<AddressEntity>
{
    public void Configure(EntityTypeBuilder<AddressEntity> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.Street).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Street2).HasMaxLength(200);
        builder.Property(a => a.City).IsRequired().HasMaxLength(100);
        builder.Property(a => a.State).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PostalCode).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Country).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Label).HasMaxLength(50);
        builder.Property(a => a.IsDefault).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);

        
        builder.HasIndex(a => a.UserId);

        // sicuramente ci saranno tante query con  questa combinazione 
        // per sicurezza  meglio mettere un indce 
        builder.HasIndex(a => new { a.UserId, a.IsDefault });
    }
}
