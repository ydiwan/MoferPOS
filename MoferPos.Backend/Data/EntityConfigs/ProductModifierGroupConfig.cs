using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoferPOS.Backend.Domain.Entities;

namespace MoferPOS.Backend.Data.EntityConfigs;

public class ProductModifierGroupConfig : IEntityTypeConfiguration<ProductModifierGroup>
{
    public void Configure(EntityTypeBuilder<ProductModifierGroup> b)
    {
        b.ToTable("ProductModifierGroups");

        b.HasKey(x => x.Id);

        b.HasIndex(x => new { x.ProductId, x.ModifierGroupId }).IsUnique();

        b.HasOne(x => x.Product)
            .WithMany(p => p.ProductModifierGroups)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ModifierGroup)
            .WithMany(g => g.ProductModifierGroups)
            .HasForeignKey(x => x.ModifierGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
