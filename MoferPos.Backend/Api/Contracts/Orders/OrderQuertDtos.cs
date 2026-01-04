namespace MoferPOS.Backend.Api.Contracts.Orders;

public sealed record OrderListResponse(
    Guid OrganizationId,
    Guid? LocationId,
    string? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<OrderListItemDto> Items
);

public sealed record OrderListItemDto(
    Guid OrderId,
    Guid LocationId,
    string LocationName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    string? ExternalOrderRef,
    PaymentSummaryDto? LatestPayment
);

public sealed record PaymentSummaryDto(
    Guid PaymentId,
    string Method,
    string Status,
    decimal Amount,
    string? TerminalTransactionRef,
    string? ResponseCode,
    string? CardBrand,
    string? Last4,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CapturedAt
);
