using MoferPOS.Backend.Domain.Common;

namespace MoferPOS.Backend.Domain.Entities;

public class Product : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Sku { get; set; }
    public decimal BasePrice { get; set; }   // before modifiers

    public bool IsActive { get; set; } = true;

    public ICollection<ProductModifierGroup> ProductModifierGroups { get; set; } = new List<ProductModifierGroup>();
}
