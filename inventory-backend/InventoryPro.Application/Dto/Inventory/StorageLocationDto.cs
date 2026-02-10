using InventoryPro.Domain.Enums;

namespace InventoryPro.Application.Dto.Inventory;

public class StorageLocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LocationType LocationType { get; set; }
    public int DisplayOrder { get; set; }
    public decimal? TemperatureMin { get; set; }
    public decimal? TemperatureMax { get; set; }
    public bool IsActive { get; set; }
}

public class CreateStorageLocationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LocationType LocationType { get; set; }
    public int DisplayOrder { get; set; }
    public decimal? TemperatureMin { get; set; }
    public decimal? TemperatureMax { get; set; }
}

public class UpdateStorageLocationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LocationType LocationType { get; set; }
    public int DisplayOrder { get; set; }
    public decimal? TemperatureMin { get; set; }
    public decimal? TemperatureMax { get; set; }
    public bool IsActive { get; set; }
}
