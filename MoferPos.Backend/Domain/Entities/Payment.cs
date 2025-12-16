using MoferPOS.Backend.Domain.Common;
using MoferPOS.Backend.Domain.Enums;

namespace MoferPOS.Backend.Domain.Entities;

public class Payment : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid LocationId { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public PaymentMethod Method { get; set; } = PaymentMethod.Unknown;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public decimal Amount { get; set; }

    // Semi-integrated terminal metadata only (NO PAN/track)
    public string? TerminalTransactionRef { get; set; } // processor/terminal reference
    public string? ApprovalCode { get; set; }
    public string? ResponseCode { get; set; }           // decline codes, etc.
    public string? CardBrand { get; set; }              // "VISA" etc (optional)
    public string? Last4 { get; set; }                  // optional if terminal provides (still not PAN)

    public DateTimeOffset? CapturedAt { get; set; }
}

