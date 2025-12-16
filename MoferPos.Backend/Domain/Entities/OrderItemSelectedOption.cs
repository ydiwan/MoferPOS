using MoferPOS.Backend.Domain.Common;

namespace MoferPOS.Backend.Domain.Entities;

public class OrderItemSelectedOption : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid LocationId { get; set; }

    public Guid OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;

    public Guid ModifierGroupId { get; set; }
    public Guid ModifierOptionId { get; set; }

    public string ModifierGroupNameSnapshot { get; set; } = null!;
    public string ModifierOptionNameSnapshot { get; set; } = null!;
    public decimal PriceDeltaSnapshot { get; set; } // captures option delta at time of sale
}
