using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoferPOS.Backend.Domain.Entities;

namespace MoferPOS.Backend.Data.EntityConfigs;

public class LocationConfig : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> b)
    {
        b.ToTable("Locations");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.AddressLine1).HasMaxLength(200);
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.Region).HasMaxLength(100);
        b.Property(x => x.PostalCode).HasMaxLength(20);

        b.HasIndex(x => new { x.OrganizationId, x.Name });

        b.HasOne(x => x.Organization)
            .WithMany(o => o.Locations)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
