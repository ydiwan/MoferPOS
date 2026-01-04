using Microsoft.EntityFrameworkCore;
using MoferPOS.Backend.Api.Contracts.Orders;
using MoferPOS.Backend.Data;

namespace MoferPOS.Backend.Application.Orders.Queries;

public sealed class OrderQueryService : IOrderQueryService
{
    private readonly AppDbContext _db;

    public OrderQueryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<OrderListResponse> GetOrdersAsync(
        Guid organizationId,
        Guid? locationId,
        string? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        if (organizationId == Guid.Empty)
            throw new InvalidOperationException("OrganizationId is required.");

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 25;
        if (pageSize > 200) pageSize = 200;

        if (from.HasValue && to.HasValue && from >= to)
            throw new InvalidOperationException("'from' must be earlier than 'to'.");

        // Validate org exists
        var orgExists = await _db.Organizations.AnyAsync(o => o.Id == organizationId, ct);
        if (!orgExists)
            throw new InvalidOperationException("Organization not found.");

        // Validate location belongs to org if provided
        if (locationId.HasValue)
        {
            var locOk = await _db.Locations.AnyAsync(l => l.Id == locationId && l.OrganizationId == organizationId, ct);
            if (!locOk)
                throw new InvalidOperationException("Location not found for organization.");
        }

        var query = _db.Orders
            .AsNoTracking()
            .Where(o => o.OrganizationId == organizationId);

        if (locationId.HasValue)
            query = query.Where(o => o.LocationId == locationId.Value);

        if (!string.IsNullOrWhiteSpace(status))
        {
            // Compare by string to keep this endpoint stable even if you extend enum later
            var normalized = status.Trim();
            query = query.Where(o => o.Status.ToString() == normalized);
        }

        // For history, filter by CreatedAt unless CompletedAt is explicitly desired later
        if (from.HasValue)
            query = query.Where(o => o.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(o => o.CreatedAt < to.Value);

        var totalCount = await query.CountAsync(ct);

        var orderPage = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.LocationId,
                o.Status,
                o.CreatedAt,
                o.CompletedAt,
                o.Subtotal,
                o.TaxTotal,
                o.Total,
                o.ExternalOrderRef
            })
            .ToListAsync(ct);

        var locationIds = orderPage.Select(x => x.LocationId).Distinct().ToList();
        var locationNames = await _db.Locations
            .AsNoTracking()
            .Where(l => l.OrganizationId == organizationId && locationIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, ct);

        var orderIds = orderPage.Select(x => x.Id).ToList();

        // Load latest payment per order (simple approach, still fine for v1 volumes)
        var latestPayments = await _db.Payments
            .AsNoTracking()
            .Where(p => orderIds.Contains(p.OrderId))
            .GroupBy(p => p.OrderId)
            .Select(g => g.OrderByDescending(x => x.CreatedAt).First())
            .ToListAsync(ct);

        var paymentByOrder = latestPayments.ToDictionary(p => p.OrderId, p => p);

        var items = orderPage.Select(o =>
        {
            paymentByOrder.TryGetValue(o.Id, out var p);

            PaymentSummaryDto? paymentDto = p is null ? null : new PaymentSummaryDto(
                p.Id,
                p.Method.ToString(),
                p.Status.ToString(),
                p.Amount,
                p.TerminalTransactionRef,
                p.ResponseCode,
                p.CardBrand,
                p.Last4,
                p.CreatedAt,
                p.CapturedAt
            );

            return new OrderListItemDto(
                o.Id,
                o.LocationId,
                locationNames.TryGetValue(o.LocationId, out var name) ? name : "(Unknown Location)",
                o.Status.ToString(),
                o.CreatedAt,
                o.CompletedAt,
                o.Subtotal,
                o.TaxTotal,
                o.Total,
                o.ExternalOrderRef,
                paymentDto
            );
        }).ToList();

        return new OrderListResponse(
            organizationId,
            locationId,
            string.IsNullOrWhiteSpace(status) ? null : status.Trim(),
            from,
            to,
            page,
            pageSize,
            totalCount,
            items
        );
    }
}
