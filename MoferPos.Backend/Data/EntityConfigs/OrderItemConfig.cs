using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoferPOS.Backend.Domain.Entities;

namespace MoferPOS.Backend.Data.EntityConfigs;

public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("OrderItems");

        b.HasKey(x => x.Id);

        b.Property(x => x.ProductNameSnapshot).HasMaxLength(200).IsRequired();

        b.Property(x => x.BaseUnitPriceSnapshot).HasPrecision(18, 2);
        b.Property(x => x.ModifierUnitTotalSnapshot).HasPrecision(18, 2);
        b.Property(x => x.FinalUnitPriceSnapshot).HasPrecision(18, 2);
        b.Property(x => x.LineTotalSnapshot).HasPrecision(18, 2);

        b.HasIndex(x => new { x.OrderId, x.CreatedAt });

        b.HasOne(x => x.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
