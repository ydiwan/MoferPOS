using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoferPOS.Backend.Domain.Entities;

namespace MoferPOS.Backend.Data.EntityConfigs;

public class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");

        b.HasKey(x => x.Id);

        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.Subtotal).HasPrecision(18, 2);
        b.Property(x => x.TaxTotal).HasPrecision(18, 2);
        b.Property(x => x.Total).HasPrecision(18, 2);

        b.Property(x => x.ExternalOrderRef).HasMaxLength(64);

        b.HasIndex(x => new { x.OrganizationId, x.LocationId, x.CreatedAt });
        b.HasIndex(x => new { x.OrganizationId, x.LocationId, x.ExternalOrderRef });

        b.HasOne(x => x.Location)
            .WithMany(l => l.Orders)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
