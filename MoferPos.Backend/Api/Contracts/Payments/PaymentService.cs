using Microsoft.EntityFrameworkCore;
using MoferPOS.Backend.Api.Contracts.Payments;
using MoferPOS.Backend.Data;
using MoferPOS.Backend.Domain.Enums;

namespace MoferPOS.Backend.Application.Payments;

public sealed class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;

    public PaymentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TerminalResultResponse> ApplyTerminalResultAsync(Guid paymentId, TerminalResultRequest request, CancellationToken ct)
    {
        if (paymentId == Guid.Empty)
            throw new InvalidOperationException("PaymentId is required.");

        // Load payment + order
        var payment = await _db.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

        if (payment is null)
            throw new InvalidOperationException("Payment not found.");

        var order = payment.Order;

        // Idempotency-ish behavior:
        // If already approved/declined/cancelled, return current state (don’t mutate again).
        if (payment.Status is PaymentStatus.Approved or PaymentStatus.Declined or PaymentStatus.Cancelled)
        {
            return new TerminalResultResponse(
                payment.Id,
                payment.Status.ToString(),
                order.Id,
                order.Status.ToString(),
                order.Total
            );
        }

        // Only Pending payments can accept terminal results
        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Payment is not in Pending state (current: {payment.Status}).");

        // Validate safe fields
        if (request.Last4 is not null && request.Last4.Length != 4)
            throw new InvalidOperationException("Last4 must be exactly 4 digits if provided.");

        // Apply metadata (safe fields only)
        payment.TerminalTransactionRef = TrimTo(request.TerminalTransactionRef, 128);
        payment.ApprovalCode = TrimTo(request.ApprovalCode, 32);
        payment.ResponseCode = TrimTo(request.ResponseCode, 32);
        payment.CardBrand = TrimTo(request.CardBrand, 32);
        payment.Last4 = TrimTo(request.Last4, 4);

        var now = DateTimeOffset.UtcNow;

        switch (request.Status)
        {
            case TerminalResultStatus.Approved:
                payment.Status = PaymentStatus.Approved;
                payment.CapturedAt = now;

                // Order becomes Completed only on approved payment (per your PRD)
                order.Status = OrderStatus.Completed;
                order.CompletedAt = now;
                break;

            case TerminalResultStatus.Declined:
                payment.Status = PaymentStatus.Declined;
                // Keep order as Draft so POS can retry payment or cancel
                // (later you can introduce Voided behavior)
                break;

            case TerminalResultStatus.Cancelled:
                payment.Status = PaymentStatus.Cancelled;
                // Keep order Draft; POS can restart payment or discard cart
                break;

            default:
                throw new InvalidOperationException("Unknown terminal result status.");
        }

        await _db.SaveChangesAsync(ct);

        return new TerminalResultResponse(
            payment.Id,
            payment.Status.ToString(),
            order.Id,
            order.Status.ToString(),
            order.Total
        );
    }

    private static string? TrimTo(string? value, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        return value.Length <= maxLen ? value : value[..maxLen];
    }
}

