namespace MoferPOS.Backend.Application.Pricing;

public sealed class PricingCalculator : IPricingCalculator
{
    public PricingResult Calculate(PricingRequest request)
    {
        if (request.Lines is null || request.Lines.Count == 0)
            throw new ArgumentException("PricingRequest must contain at least one line.", nameof(request));

        if (request.TaxRate < 0 || request.TaxRate > 1)
            throw new ArgumentOutOfRangeException(nameof(request.TaxRate), "TaxRate must be between 0 and 1.");

        var pricedLines = new List<PricedLine>(request.Lines.Count);

        decimal subtotal = 0m;

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(line.Quantity), "Quantity must be >= 1.");

            if (line.BaseUnitPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(line.BaseUnitPrice), "BaseUnitPrice must be >= 0.");

            var modifierUnitTotal = line.SelectedOptions.Sum(o => o.PriceDelta);
            var finalUnit = RoundMoney(line.BaseUnitPrice + modifierUnitTotal);
            var lineTotal = RoundMoney(finalUnit * line.Quantity);

            pricedLines.Add(new PricedLine
            {
                ProductName = line.ProductName,
                Quantity = line.Quantity,
                BaseUnitPrice = RoundMoney(line.BaseUnitPrice),
                ModifierUnitTotal = RoundMoney(modifierUnitTotal),
                FinalUnitPrice = finalUnit,
                LineTotal = lineTotal
            });

            subtotal += lineTotal;
        }

        subtotal = RoundMoney(subtotal);
        var tax = RoundMoney(subtotal * request.TaxRate);
        var total = RoundMoney(subtotal + tax);

        return new PricingResult
        {
            Lines = pricedLines,
            Subtotal = subtotal,
            TaxTotal = tax,
            Total = total
        };
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
