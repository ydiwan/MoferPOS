using MoferPOS.Backend.Domain.Common;

namespace MoferPOS.Backend.Domain.Entities;

public class ProductModifierGroup : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid LocationId { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid ModifierGroupId { get; set; }
    public ModifierGroup ModifierGroup { get; set; } = null!;

    // rules for selection UX + validation
    public bool IsRequired { get; set; } = false;
    public int MinSelected { get; set; } = 0;
    public int MaxSelected { get; set; } = 1;

    public int DisplayOrder { get; set; } = 0;
}
