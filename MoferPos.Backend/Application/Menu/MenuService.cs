using Microsoft.EntityFrameworkCore;
using MoferPOS.Backend.Api.Contracts;
using MoferPOS.Backend.Data;

namespace MoferPOS.Backend.Application.Menu;

public sealed class MenuService : IMenuService
{
    private readonly AppDbContext _db;

    public MenuService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MenuResponse> GetMenuAsync(Guid organizationId, Guid locationId, CancellationToken ct)
    {
        // Validate location belongs to org
        var locationExists = await _db.Locations
            .AnyAsync(l => l.Id == locationId && l.OrganizationId == organizationId, ct);

        if (!locationExists)
            throw new InvalidOperationException("Location not found for organization.");

        // Pull products + their modifier group attachments + group options
        var products = await _db.Products
            .Where(p => p.OrganizationId == organizationId && p.LocationId == locationId)
            .OrderBy(p => p.Name)
            .Select(p => new MenuProductDto(
                p.Id,
                p.Name,
                p.Sku,
                p.BasePrice,
                p.IsActive,
                p.ProductModifierGroups
                    .OrderBy(j => j.DisplayOrder)
                    .Select(j => new MenuProductModifierGroupDto(
                        j.ModifierGroupId,
                        j.ModifierGroup.Name,
                        j.IsRequired,
                        j.MinSelected,
                        j.MaxSelected,
                        j.DisplayOrder,
                        j.ModifierGroup.Options
                            .OrderBy(o => o.Name)
                            .Select(o => new MenuModifierOptionDto(
                                o.Id,
                                o.Name,
                                o.PriceDelta,
                                o.IsActive
                            ))
                            .ToList()
                    ))
                    .ToList()
            ))
            .ToListAsync(ct);

        return new MenuResponse(organizationId, locationId, products);
    }
}
