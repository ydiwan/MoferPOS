using MoferPOS.Backend.Api.Contracts.Payments;

namespace MoferPOS.Backend.Application.Payments;

public interface IPaymentService
{
    Task<TerminalResultResponse> ApplyTerminalResultAsync(Guid paymentId, TerminalResultRequest request, CancellationToken ct);
}
