using MoferPOS.Backend.Domain.Common;
using MoferPOS.Backend.Domain.Enums;

namespace MoferPOS.Backend.Domain.Entities;

public class Order : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;

    // Helpful for multi-register, idempotency, and audit (v1 can leave null)
    public string? ExternalOrderRef { get; set; } // POS-generated stable ID for retries

    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    // Totals captured at completion time
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
