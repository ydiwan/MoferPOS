using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoferPOS.Backend.Domain.Entities;

namespace MoferPOS.Backend.Data.EntityConfigs;

public class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments");

        b.HasKey(x => x.Id);

        b.Property(x => x.Method).IsRequired();
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);

        b.Property(x => x.TerminalTransactionRef).HasMaxLength(128);
        b.Property(x => x.ApprovalCode).HasMaxLength(32);
        b.Property(x => x.ResponseCode).HasMaxLength(32);
        b.Property(x => x.CardBrand).HasMaxLength(32);
        b.Property(x => x.Last4).HasMaxLength(4);

        b.HasIndex(x => new { x.OrderId, x.CreatedAt });

        b.HasOne(x => x.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
