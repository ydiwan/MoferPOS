namespace MoferPOS.Backend.Application.Pricing;

public sealed class PricingRequest
{
    public required IReadOnlyList<PricingLine> Lines { get; init; }

    // v1 simple tax (you can evolve into per-location tax rules later)
    public decimal TaxRate { get; init; } = 0.00m; // 0.06m for 6%
}

public sealed class PricingLine
{
    public required string ProductName { get; init; }
    public int Quantity { get; init; } = 1;

    public decimal BaseUnitPrice { get; init; }

    // Sum of selected option deltas PER UNIT (not multiplied by quantity yet)
    public IReadOnlyList<SelectedOptionDelta> SelectedOptions { get; init; } = Array.Empty<SelectedOptionDelta>();
}

public sealed class SelectedOptionDelta
{
    public required string GroupName { get; init; }
    public required string OptionName { get; init; }
    public decimal PriceDelta { get; init; }
}
