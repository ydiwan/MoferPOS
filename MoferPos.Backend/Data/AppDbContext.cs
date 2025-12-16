using Microsoft.EntityFrameworkCore;
using MoferPOS.Backend.Domain.Common;
using MoferPOS.Backend.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MoferPOS.Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ModifierGroup> ModifierGroups => Set<ModifierGroup>();
    public DbSet<ModifierOption> ModifierOptions => Set<ModifierOption>();
    public DbSet<ProductModifierGroup> ProductModifierGroups => Set<ProductModifierGroup>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemSelectedOption> OrderItemSelectedOptions => Set<OrderItemSelectedOption>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global soft delete filter for all ISoftDeletable entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, ISoftDeletable
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDeleteRules();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDeleteRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditAndSoftDeleteRules()
    {
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditableEntity auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = utcNow;
                    auditable.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    // Convert hard delete -> soft delete
                    entry.State = EntityState.Modified;
                    auditable.IsDeleted = true;
                    auditable.DeletedAt = utcNow;
                    auditable.UpdatedAt = utcNow;
                }
            }
        }
    }
}
