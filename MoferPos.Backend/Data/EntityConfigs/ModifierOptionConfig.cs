using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoferPOS.Backend.Domain.Entities;

namespace MoferPOS.Backend.Data.EntityConfigs;

public class ModifierOptionConfig : IEntityTypeConfiguration<ModifierOption>
{
    public void Configure(EntityTypeBuilder<ModifierOption> b)
    {
        b.ToTable("ModifierOptions");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.PriceDelta).HasPrecision(18, 2);

        b.HasIndex(x => new { x.ModifierGroupId, x.Name });

        b.HasOne(x => x.ModifierGroup)
            .WithMany(g => g.Options)
            .HasForeignKey(x => x.ModifierGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
