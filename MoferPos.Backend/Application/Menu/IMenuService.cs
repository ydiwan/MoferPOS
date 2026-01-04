using MoferPOS.Backend.Api.Contracts;

namespace MoferPOS.Backend.Application.Menu;

public interface IMenuService
{
    Task<MenuResponse> GetMenuAsync(Guid organizationId, Guid locationId, CancellationToken ct);
}
