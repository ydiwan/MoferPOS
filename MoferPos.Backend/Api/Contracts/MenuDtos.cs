namespace MoferPOS.Backend.Api.Contracts;

public sealed record MenuResponse(
    Guid OrganizationId,
    Guid LocationId,
    IReadOnlyList<MenuProductDto> Products
);

public sealed record MenuProductDto(
    Guid ProductId,
    string Name,
    string? Sku,
    decimal BasePrice,
    bool IsActive,
    IReadOnlyList<MenuProductModifierGroupDto> ModifierGroups
);

public sealed record MenuProductModifierGroupDto(
    Guid ModifierGroupId,
    string Name,
    bool IsRequired,
    int MinSelected,
    int MaxSelected,
    int DisplayOrder,
    IReadOnlyList<MenuModifierOptionDto> Options
);

public sealed record MenuModifierOptionDto(
    Guid ModifierOptionId,
    string Name,
    decimal PriceDelta,
    bool IsActive
);
