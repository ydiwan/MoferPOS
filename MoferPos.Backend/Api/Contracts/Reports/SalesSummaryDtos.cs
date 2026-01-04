namespace MoferPOS.Backend.Api.Contracts.Reports;

public sealed record SalesSummaryResponse(
    Guid OrganizationId,
    DateTimeOffset From,
    DateTimeOffset To,
    Guid? LocationId,
    string TimezoneNote,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    int CompletedOrderCount,
    decimal AverageOrderTotal,
    IReadOnlyList<LocationSalesSummaryRow> ByLocation
);

public sealed record LocationSalesSummaryRow(
    Guid LocationId,
    string LocationName,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    int CompletedOrderCount
);

