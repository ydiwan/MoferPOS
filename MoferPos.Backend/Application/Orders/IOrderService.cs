using MoferPOS.Backend.Api.Contracts;

namespace MoferPOS.Backend.Application.Orders;

public interface IOrderService
{
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct);
}
