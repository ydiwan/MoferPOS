using MoferPOS.Backend.Domain.Common;

namespace MoferPOS.Backend.Domain.Entities;

public class ModifierGroup : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public ICollection<ModifierOption> Options { get; set; } = new List<ModifierOption>();
    public ICollection<ProductModifierGroup> ProductModifierGroups { get; set; } = new List<ProductModifierGroup>();
}
