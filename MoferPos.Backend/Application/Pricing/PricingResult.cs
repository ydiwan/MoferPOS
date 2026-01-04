namespace MoferPOS.Backend.Application.Pricing;

public sealed class PricingResult
{
    public required IReadOnlyList<PricedLine> Lines { get; init; }
    public decimal Subtotal { get; init; }
    public decimal TaxTotal { get; init; }
    public decimal Total { get; init; }
}

public sealed class PricedLine
{
    public required string ProductName { get; init; }
    public int Quantity { get; init; }

    public decimal BaseUnitPrice { get; init; }
    public decimal ModifierUnitTotal { get; init; }
    public decimal FinalUnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

