using MoferPOS.Backend.Api.Contracts.Reports;

namespace MoferPOS.Backend.Application.Reports;

public interface IReportService
{
    Task<SalesSummaryResponse> GetSalesSummaryAsync(
        Guid organizationId,
        Guid? locationId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct);
}
