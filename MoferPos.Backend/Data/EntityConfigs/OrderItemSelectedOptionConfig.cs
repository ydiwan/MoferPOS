using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoferPOS.Backend.Domain.Entities;

namespace MoferPOS.Backend.Data.EntityConfigs;

public class OrderItemSelectedOptionConfig : IEntityTypeConfiguration<OrderItemSelectedOption>
{
    public void Configure(EntityTypeBuilder<OrderItemSelectedOption> b)
    {
        b.ToTable("OrderItemSelectedOptions");

        b.HasKey(x => x.Id);

        b.Property(x => x.ModifierGroupNameSnapshot).HasMaxLength(200).IsRequired();
        b.Property(x => x.ModifierOptionNameSnapshot).HasMaxLength(200).IsRequired();
        b.Property(x => x.PriceDeltaSnapshot).HasPrecision(18, 2);

        b.HasIndex(x => new { x.OrderItemId, x.ModifierOptionId });

        b.HasOne(x => x.OrderItem)
            .WithMany(i => i.SelectedOptions)
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
