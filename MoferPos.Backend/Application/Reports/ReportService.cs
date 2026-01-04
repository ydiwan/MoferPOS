using Microsoft.EntityFrameworkCore;
using MoferPOS.Backend.Api.Contracts.Reports;
using MoferPOS.Backend.Data;
using MoferPOS.Backend.Domain.Enums;

namespace MoferPOS.Backend.Application.Reports;

public sealed class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SalesSummaryResponse> GetSalesSummaryAsync(
        Guid organizationId,
        Guid? locationId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        if (organizationId == Guid.Empty)
            throw new InvalidOperationException("OrganizationId is required.");

        if (from >= to)
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

        // Only Completed orders count in analytics.
        // Use CompletedAt (not CreatedAt) because completion time is what matters for revenue recognition.
        var baseQuery = _db.Orders
            .AsNoTracking()
            .Where(o =>
                o.OrganizationId == organizationId &&
                o.Status == OrderStatus.Completed &&
                o.CompletedAt != null &&
                o.CompletedAt >= from &&
                o.CompletedAt < to);

        if (locationId.HasValue)
            baseQuery = baseQuery.Where(o => o.LocationId == locationId.Value);

        var totals = await baseQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Subtotal = g.Sum(x => x.Subtotal),
                TaxTotal = g.Sum(x => x.TaxTotal),
                Total = g.Sum(x => x.Total),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        var byLocation = await baseQuery
            .GroupBy(o => o.LocationId)
            .Select(g => new
            {
                LocationId = g.Key,
                Subtotal = g.Sum(x => x.Subtotal),
                TaxTotal = g.Sum(x => x.TaxTotal),
                Total = g.Sum(x => x.Total),
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Join location names
        var locationIds = byLocation.Select(x => x.LocationId).Distinct().ToList();
        var locationNames = await _db.Locations
            .AsNoTracking()
            .Where(l => l.OrganizationId == organizationId && locationIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, ct);

        var rows = byLocation
            .OrderByDescending(x => x.Total)
            .Select(x => new LocationSalesSummaryRow(
                x.LocationId,
                locationNames.TryGetValue(x.LocationId, out var name) ? name : "(Unknown Location)",
                RoundMoney(x.Subtotal),
                RoundMoney(x.TaxTotal),
                RoundMoney(x.Total),
                x.Count
            ))
            .ToList();

        var subtotal = RoundMoney(totals?.Subtotal ?? 0m);
        var taxTotal = RoundMoney(totals?.TaxTotal ?? 0m);
        var total = RoundMoney(totals?.Total ?? 0m);
        var count = totals?.Count ?? 0;

        var avg = count == 0 ? 0m : RoundMoney(total / count);

        return new SalesSummaryResponse(
            organizationId,
            from,
            to,
            locationId,
            "All timestamps are treated as UTC DateTimeOffset in v1.",
            subtotal,
            taxTotal,
            total,
            count,
            avg,
            rows
        );
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
