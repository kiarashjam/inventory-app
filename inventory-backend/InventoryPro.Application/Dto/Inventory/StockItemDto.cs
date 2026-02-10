using InventoryPro.Domain.Enums;

namespace InventoryPro.Application.Dto.Inventory;

public class StockItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public UnitOfMeasurement BaseUnitOfMeasurement { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal MinimumThreshold { get; set; }
    public decimal? ParLevel { get; set; }
    public decimal? MaximumCapacity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal AverageCostPrice { get; set; }
    public int? PrimarySupplierId { get; set; }
    public string? PrimarySupplierName { get; set; }
    public string? Barcode { get; set; }
    public int? PrimaryStorageLocationId { get; set; }
    public string? PrimaryStorageLocationName { get; set; }
    public bool IsActive { get; set; }
    public bool IsPerishable { get; set; }
    public int? DefaultExpirationDays { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string StockStatus => CurrentQuantity <= 0 ? "OutOfStock" :
        CurrentQuantity < MinimumThreshold ? "Low" :
        ParLevel.HasValue && CurrentQuantity < ParLevel.Value ? "BelowPar" :
        MaximumCapacity.HasValue && CurrentQuantity > MaximumCapacity.Value ? "Overstock" : "InStock";
}
