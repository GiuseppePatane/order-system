using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Product.Infrastructure.EF.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Core.Domain.Product>
{
    public void Configure(EntityTypeBuilder<Core.Domain.Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).IsRequired().HasPrecision(10,2);
        builder.Property(p => p.CreatedAt).IsRequired();

        
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        builder.HasIndex(x=> x.Name);
        builder.HasIndex(x => x.Sku).IsUnique();
    }
}