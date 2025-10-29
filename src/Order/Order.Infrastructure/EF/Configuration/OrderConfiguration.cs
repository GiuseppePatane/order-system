using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Order.Infrastructure.EF.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Core.Domain.Order>
{
    public void Configure(EntityTypeBuilder<Core.Domain.Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.ShippingAddressId)
            .IsRequired();

        builder.Property(o => o.BillingAddressId);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        // Configure owned collection of order items
        builder.HasMany(o => o.Items)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(o => o.TotalAmount); // Computed property

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
    }
}
