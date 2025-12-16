using MoferPOS.Backend.Domain.Common;

namespace MoferPOS.Backend.Domain.Entities;

public class Organization : AuditableEntity
{
    public string Name { get; set; } = null!;

    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
