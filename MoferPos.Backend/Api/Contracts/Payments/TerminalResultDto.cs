namespace MoferPOS.Backend.Api.Contracts.Payments;

public enum TerminalResultStatus
{
    Approved = 1,
    Declined = 2,
    Cancelled = 3
}

public sealed class TerminalResultRequest
{
    public TerminalResultStatus Status { get; init; }

    // Semi-integrated metadata only (NO PAN/track data)
    public string? TerminalTransactionRef { get; init; } // processor/terminal reference
    public string? ApprovalCode { get; init; }
    public string? ResponseCode { get; init; }
    public string? CardBrand { get; init; }
    public string? Last4 { get; init; }
}

public sealed record TerminalResultResponse(
    Guid PaymentId,
    string PaymentStatus,
    Guid OrderId,
    string OrderStatus,
    decimal OrderTotal
);

