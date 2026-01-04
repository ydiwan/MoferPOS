using MoferPOS.Backend.Api.Contracts.Orders;

namespace MoferPOS.Backend.Application.Orders.Queries;

public interface IOrderQueryService
{
    Task<OrderListResponse> GetOrdersAsync(
        Guid organizationId,
        Guid? locationId,
        string? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken ct);
}
