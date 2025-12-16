using MoferPOS.Backend.Domain.Common;

namespace MoferPOS.Backend.Domain.Entities;

public class OrderItem : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid LocationId { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string ProductNameSnapshot { get; set; } = null!;

    // Final pricing at time of sale (rule: store final unit price)
    public int Quantity { get; set; } = 1;
    public decimal BaseUnitPriceSnapshot { get; set; }    // product base at time of sale
    public decimal ModifierUnitTotalSnapshot { get; set; } // sum of selected option deltas per unit
    public decimal FinalUnitPriceSnapshot { get; set; }   // base + modifiers (per unit)

    public decimal LineTotalSnapshot { get; set; }        // FinalUnitPriceSnapshot * Quantity

    public ICollection<OrderItemSelectedOption> SelectedOptions { get; set; } = new List<OrderItemSelectedOption>();
}
