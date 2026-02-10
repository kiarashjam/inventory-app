using InventoryPro.Domain.Enums;

namespace InventoryPro.Application.Dto.Inventory;

public class UpdateStockItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public UnitOfMeasurement BaseUnitOfMeasurement { get; set; }
    public decimal MinimumThreshold { get; set; }
    public decimal? ParLevel { get; set; }
    public decimal? MaximumCapacity { get; set; }
    public decimal CostPrice { get; set; }
    public int? PrimarySupplierId { get; set; }
    public string? Barcode { get; set; }
    public int? PrimaryStorageLocationId { get; set; }
    public bool IsActive { get; set; }
    public bool IsPerishable { get; set; }
    public int? DefaultExpirationDays { get; set; }
    public string? Notes { get; set; }
}
