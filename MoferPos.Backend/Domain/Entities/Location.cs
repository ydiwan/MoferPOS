using MoferPOS.Backend.Domain.Common;

namespace MoferPOS.Backend.Domain.Entities;

public class Location : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }

    // menu + ordering
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

