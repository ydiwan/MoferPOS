namespace MoferPOS.Backend.Api.Contracts;

public sealed class CreateOrderRequest
{
    public Guid OrganizationId { get; init; }
    public Guid LocationId { get; init; }

    // REQUIRED for idempotency (POS should generate a stable ID per attempt)
    public required string ExternalOrderRef { get; init; }

    // v1: allow POS to pass tax rate; later this comes from location tax config
    public decimal TaxRate { get; init; } = 0.00m;

    public required IReadOnlyList<CreateOrderLine> Lines { get; init; }
}

public sealed class CreateOrderLine
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; } = 1;

    // Selected modifier option IDs across all groups attached to this product
    public IReadOnlyList<Guid> SelectedOptionIds { get; init; } = Array.Empty<Guid>();
}

public sealed record CreateOrderResponse(
    Guid OrderId,
    string Status,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    Guid PaymentId,
    string PaymentStatus
);
