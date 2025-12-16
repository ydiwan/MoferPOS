using MoferPOS.Backend.Domain.Common;

namespace MoferPOS.Backend.Domain.Entities;

public class ModifierOption : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid LocationId { get; set; }

    public Guid ModifierGroupId { get; set; }
    public ModifierGroup ModifierGroup { get; set; } = null!;

    public string Name { get; set; } = null!;
    public decimal PriceDelta { get; set; } // e.g., +0.50 for oat milk, +1.00 for large

    public bool IsActive { get; set; } = true;
}
