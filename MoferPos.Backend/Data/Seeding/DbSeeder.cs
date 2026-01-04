using Microsoft.EntityFrameworkCore;
using MoferPOS.Backend.Domain.Entities;

namespace MoferPOS.Backend.Data.Seeding;

public static class DbSeeder
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        // Already seeded?
        if (await db.Organizations.AnyAsync())
            return;

        // Org + Location
        var org = new Organization { Name = "Mofer Coffee" };
        var location = new Location
        {
            Organization = org,
            Name = "Mofer Coffee - Main",
            AddressLine1 = "123 Coffee St",
            City = "Richmond",
            Region = "VA",
            PostalCode = "23220"
        };

        // Modifier Groups
        var sizeGroup = new ModifierGroup
        {
            OrganizationId = org.Id,
            Location = location,
            Name = "Size",
            IsActive = true
        };

        var milkGroup = new ModifierGroup
        {
            OrganizationId = org.Id,
            Location = location,
            Name = "Milk",
            IsActive = true
        };

        var extrasGroup = new ModifierGroup
        {
            OrganizationId = org.Id,
            Location = location,
            Name = "Extras",
            IsActive = true
        };

        // Modifier Options (PriceDelta drives modifier pricing)
        var sizeSmall = new ModifierOption
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            ModifierGroup = sizeGroup,
            Name = "Small",
            PriceDelta = 0.00m
        };
        var sizeMedium = new ModifierOption
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            ModifierGroup = sizeGroup,
            Name = "Medium",
            PriceDelta = 0.50m
        };
        var sizeLarge = new ModifierOption
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            ModifierGroup = sizeGroup,
            Name = "Large",
            PriceDelta = 1.00m
        };

        var milkWhole = new ModifierOption
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            ModifierGroup = milkGroup,
            Name = "Whole Milk",
            PriceDelta = 0.00m
        };
        var milkOat = new ModifierOption
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            ModifierGroup = milkGroup,
            Name = "Oat Milk",
            PriceDelta = 0.75m
        };

        var extraShot = new ModifierOption
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            ModifierGroup = extrasGroup,
            Name = "Extra Shot",
            PriceDelta = 1.25m
        };
        var vanilla = new ModifierOption
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            ModifierGroup = extrasGroup,
            Name = "Vanilla Syrup",
            PriceDelta = 0.60m
        };

        // Products
        var latte = new Product
        {
            OrganizationId = org.Id,
            Location = location,
            Name = "Latte",
            Sku = "LATTE",
            BasePrice = 4.50m,
            IsActive = true
        };

        var drip = new Product
        {
            OrganizationId = org.Id,
            Location = location,
            Name = "Drip Coffee",
            Sku = "DRIP",
            BasePrice = 3.00m,
            IsActive = true
        };

        // Attach modifier groups to products (rules for selection)
        var latteSize = new ProductModifierGroup
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            Product = latte,
            ModifierGroup = sizeGroup,
            IsRequired = true,
            MinSelected = 1,
            MaxSelected = 1,
            DisplayOrder = 1
        };

        var latteMilk = new ProductModifierGroup
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            Product = latte,
            ModifierGroup = milkGroup,
            IsRequired = true,
            MinSelected = 1,
            MaxSelected = 1,
            DisplayOrder = 2
        };

        var latteExtras = new ProductModifierGroup
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            Product = latte,
            ModifierGroup = extrasGroup,
            IsRequired = false,
            MinSelected = 0,
            MaxSelected = 3,
            DisplayOrder = 3
        };

        var dripSize = new ProductModifierGroup
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            Product = drip,
            ModifierGroup = sizeGroup,
            IsRequired = true,
            MinSelected = 1,
            MaxSelected = 1,
            DisplayOrder = 1
        };

        db.Organizations.Add(org);
        db.Locations.Add(location);

        db.ModifierGroups.AddRange(sizeGroup, milkGroup, extrasGroup);
        db.ModifierOptions.AddRange(sizeSmall, sizeMedium, sizeLarge, milkWhole, milkOat, extraShot, vanilla);

        db.Products.AddRange(latte, drip);
        db.ProductModifierGroups.AddRange(latteSize, latteMilk, latteExtras, dripSize);

        await db.SaveChangesAsync();
    }
}

