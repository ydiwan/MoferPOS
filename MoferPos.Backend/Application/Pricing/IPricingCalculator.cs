namespace MoferPOS.Backend.Application.Pricing;

public interface IPricingCalculator
{
    PricingResult Calculate(PricingRequest request);
}
