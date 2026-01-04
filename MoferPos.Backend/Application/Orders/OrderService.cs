using Microsoft.EntityFrameworkCore;
using MoferPOS.Backend.Api.Contracts;
using MoferPOS.Backend.Application.Pricing;
using MoferPOS.Backend.Data;
using MoferPOS.Backend.Domain.Entities;
using MoferPOS.Backend.Domain.Enums;

namespace MoferPOS.Backend.Application.Orders;

public sealed class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IPricingCalculator _pricing;

    public OrderService(AppDbContext db, IPricingCalculator pricing)
    {
        _db = db;
        _pricing = pricing;
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
    {
        ValidateCreateOrderRequest(request);

        // Idempotency: return existing order if already created with this ExternalOrderRef
        var existing = await _db.Orders
            .Include(o => o.Payments)
            .Where(o =>
                o.OrganizationId == request.OrganizationId &&
                o.LocationId == request.LocationId &&
                o.ExternalOrderRef == request.ExternalOrderRef)
            .FirstOrDefaultAsync(ct);

        if (existing is not null)
        {
            var existingPayment = existing.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if (existingPayment is null)
                throw new InvalidOperationException("Existing order has no payment record.");

            return new CreateOrderResponse(
                existing.Id,
                existing.Status.ToString(),
                existing.Subtotal,
                existing.TaxTotal,
                existing.Total,
                existingPayment.Id,
                existingPayment.Status.ToString()
            );
        }

        // Validate location belongs to org
        var locationExists = await _db.Locations
            .AnyAsync(l => l.Id == request.LocationId && l.OrganizationId == request.OrganizationId, ct);

        if (!locationExists)
            throw new InvalidOperationException("Location not found for organization.");

        // Load all products referenced + their modifier rules + options
        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Where(p =>
                p.OrganizationId == request.OrganizationId &&
                p.LocationId == request.LocationId &&
                productIds.Contains(p.Id))
            .Include(p => p.ProductModifierGroups)
                .ThenInclude(j => j.ModifierGroup)
                    .ThenInclude(g => g.Options)
            .ToListAsync(ct);

        if (products.Count != productIds.Count)
            throw new InvalidOperationException("One or more products were not found for the location.");

        // Build priced lines (also validate modifier constraints)
        var pricingLines = new List<PricingLine>(request.Lines.Count);

        foreach (var line in request.Lines)
        {
            var product = products.First(p => p.Id == line.ProductId);

            if (!product.IsActive)
                throw new InvalidOperationException($"Product '{product.Name}' is not active.");

            if (line.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be >= 1.");

            var selectedOptionIds = (line.SelectedOptionIds ?? Array.Empty<Guid>()).ToList();

            // Validate selections against product's attached modifier groups & their rules
            var selectedOptionEntities = ValidateAndResolveSelectedOptions(product, selectedOptionIds);

            pricingLines.Add(new PricingLine
            {
                ProductName = product.Name,
                Quantity = line.Quantity,
                BaseUnitPrice = product.BasePrice,
                SelectedOptions = selectedOptionEntities
                    .Select(o => new SelectedOptionDelta
                    {
                        GroupName = o.GroupName,
                        OptionName = o.OptionName,
                        PriceDelta = o.PriceDelta
                    })
                    .ToList()
            });
        }

        var pricingResult = _pricing.Calculate(new PricingRequest
        {
            Lines = pricingLines,
            TaxRate = request.TaxRate
        });

        // Create order + snapshots
        var order = new Order
        {
            OrganizationId = request.OrganizationId,
            LocationId = request.LocationId,
            ExternalOrderRef = request.ExternalOrderRef,
            Status = OrderStatus.Draft, // v1: payment is pending, so not completed yet
            Subtotal = pricingResult.Subtotal,
            TaxTotal = pricingResult.TaxTotal,
            Total = pricingResult.Total
        };

        // Persist items with snapshot pricing and selected option snapshots
        for (int i = 0; i < request.Lines.Count; i++)
        {
            var reqLine = request.Lines[i];
            var pricedLine = pricingResult.Lines[i];
            var product = products.First(p => p.Id == reqLine.ProductId);

            var selectedOptionEntities = ValidateAndResolveSelectedOptions(product, reqLine.SelectedOptionIds);

            var item = new OrderItem
            {
                OrganizationId = request.OrganizationId,
                LocationId = request.LocationId,
                Order = order,
                ProductId = product.Id,
                ProductNameSnapshot = product.Name,
                Quantity = pricedLine.Quantity,
                BaseUnitPriceSnapshot = pricedLine.BaseUnitPrice,
                ModifierUnitTotalSnapshot = pricedLine.ModifierUnitTotal,
                FinalUnitPriceSnapshot = pricedLine.FinalUnitPrice,
                LineTotalSnapshot = pricedLine.LineTotal
            };

            foreach (var sel in selectedOptionEntities)
            {
                item.SelectedOptions.Add(new OrderItemSelectedOption
                {
                    OrganizationId = request.OrganizationId,
                    LocationId = request.LocationId,
                    ModifierGroupId = sel.ModifierGroupId,
                    ModifierOptionId = sel.ModifierOptionId,
                    ModifierGroupNameSnapshot = sel.GroupName,
                    ModifierOptionNameSnapshot = sel.OptionName,
                    PriceDeltaSnapshot = sel.PriceDelta
                });
            }

            order.Items.Add(item);
        }

        // Create payment record (pending)
        var payment = new Payment
        {
            OrganizationId = request.OrganizationId,
            LocationId = request.LocationId,
            Order = order,
            Method = PaymentMethod.Card,      // v1 assumption; later allow cash/card split
            Status = PaymentStatus.Pending,
            Amount = order.Total
        };

        order.Payments.Add(payment);

        _db.Orders.Add(order);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // If unique index hits due to retry race, fetch and return existing
            var existingAfterConflict = await _db.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o =>
                    o.OrganizationId == request.OrganizationId &&
                    o.LocationId == request.LocationId &&
                    o.ExternalOrderRef == request.ExternalOrderRef, ct);

            if (existingAfterConflict is not null)
            {
                var p = existingAfterConflict.Payments.OrderByDescending(x => x.CreatedAt).First();
                return new CreateOrderResponse(
                    existingAfterConflict.Id,
                    existingAfterConflict.Status.ToString(),
                    existingAfterConflict.Subtotal,
                    existingAfterConflict.TaxTotal,
                    existingAfterConflict.Total,
                    p.Id,
                    p.Status.ToString()
                );
            }

            throw new InvalidOperationException("Failed to create order.", ex);
        }

        return new CreateOrderResponse(
            order.Id,
            order.Status.ToString(),
            order.Subtotal,
            order.TaxTotal,
            order.Total,
            payment.Id,
            payment.Status.ToString()
        );
    }

    private static void ValidateCreateOrderRequest(CreateOrderRequest request)
    {
        if (request.OrganizationId == Guid.Empty)
            throw new InvalidOperationException("OrganizationId is required.");

        if (request.LocationId == Guid.Empty)
            throw new InvalidOperationException("LocationId is required.");

        if (string.IsNullOrWhiteSpace(request.ExternalOrderRef))
            throw new InvalidOperationException("ExternalOrderRef is required.");

        if (request.ExternalOrderRef.Length > 64)
            throw new InvalidOperationException("ExternalOrderRef must be <= 64 characters.");

        if (request.Lines is null || request.Lines.Count == 0)
            throw new InvalidOperationException("Order must contain at least one line.");

        if (request.TaxRate < 0 || request.TaxRate > 1)
            throw new InvalidOperationException("TaxRate must be between 0 and 1.");
    }

    private sealed record ResolvedSelection(
        Guid ModifierGroupId,
        Guid ModifierOptionId,
        string GroupName,
        string OptionName,
        decimal PriceDelta
    );

    private static List<ResolvedSelection> ValidateAndResolveSelectedOptions(Product product, IReadOnlyList<Guid>? selectedOptionIds)
    {
        selectedOptionIds ??= Array.Empty<Guid>();

        var attachments = product.ProductModifierGroups
            .OrderBy(a => a.DisplayOrder)
            .ToList();

        // Flatten all valid options for this product (only those belonging to attached groups)
        var optionsById = attachments
            .SelectMany(a => a.ModifierGroup.Options.Select(o => new
            {
                GroupId = a.ModifierGroupId,
                GroupName = a.ModifierGroup.Name,
                OptionId = o.Id,
                OptionName = o.Name,
                PriceDelta = o.PriceDelta,
                IsActive = o.IsActive
            }))
            .ToDictionary(x => x.OptionId, x => x);

        // Ensure every selected option is valid for this product
        foreach (var optId in selectedOptionIds)
        {
            if (!optionsById.ContainsKey(optId))
                throw new InvalidOperationException($"Selected option '{optId}' is not valid for product '{product.Name}'.");
        }

        // Group selections by modifier group
        var selectedByGroup = selectedOptionIds
            .Select(id => optionsById[id])
            .GroupBy(x => x.GroupId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Enforce rules for each attached modifier group
        foreach (var att in attachments)
        {
            var groupId = att.ModifierGroupId;
            var selectedCount = selectedByGroup.TryGetValue(groupId, out var list) ? list.Count : 0;

            // Active check for selected options
            if (list is not null && list.Any(x => !x.IsActive))
                throw new InvalidOperationException($"One or more selected options in '{att.ModifierGroup.Name}' are not active.");

            // Required/min/max enforcement
            if (att.IsRequired && selectedCount < Math.Max(1, att.MinSelected))
                throw new InvalidOperationException($"Modifier group '{att.ModifierGroup.Name}' requires at least {Math.Max(1, att.MinSelected)} selection(s).");

            if (selectedCount < att.MinSelected)
                throw new InvalidOperationException($"Modifier group '{att.ModifierGroup.Name}' requires at least {att.MinSelected} selection(s).");

            if (selectedCount > att.MaxSelected)
                throw new InvalidOperationException($"Modifier group '{att.ModifierGroup.Name}' allows at most {att.MaxSelected} selection(s).");
        }

        // Return resolved selections in a stable order
        var resolved = new List<ResolvedSelection>();

        foreach (var att in attachments)
        {
            if (!selectedByGroup.TryGetValue(att.ModifierGroupId, out var list))
                continue;

            foreach (var o in list.OrderBy(x => x.OptionName))
            {
                resolved.Add(new ResolvedSelection(
                    att.ModifierGroupId,
                    o.OptionId,
                    o.GroupName,
                    o.OptionName,
                    o.PriceDelta
                ));
            }
        }

        return resolved;
    }
}
