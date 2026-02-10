using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class StorageLocation
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public LocationType LocationType { get; set; }
    public int DisplayOrder { get; set; }
    public decimal? TemperatureMin { get; set; }
    public decimal? TemperatureMax { get; set; }
    public bool IsActive { get; set; }

    public Organization Organization { get; set; } = null!;
}
