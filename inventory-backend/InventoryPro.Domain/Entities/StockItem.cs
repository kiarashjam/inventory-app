using System.ComponentModel.DataAnnotations;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class StockItem
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public required string Name { get; set; }
    public string? SKU { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public UnitOfMeasurement BaseUnitOfMeasurement { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal MinimumThreshold { get; set; }
    public decimal? ParLevel { get; set; }
    public decimal? MaximumCapacity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal AverageCostPrice { get; set; }
    public int? PrimarySupplierId { get; set; }
    public string? Barcode { get; set; }
    public int? PrimaryStorageLocationId { get; set; }
    public bool IsActive { get; set; }
    public bool IsPerishable { get; set; }
    public int? DefaultExpirationDays { get; set; }
    public string? Notes { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public Organization Organization { get; set; } = null!;
    public StockCategory Category { get; set; } = null!;
    public Supplier? PrimarySupplier { get; set; }
    public StorageLocation? PrimaryStorageLocation { get; set; }
    public ICollection<StockItemAllergen> AllergenTags { get; set; } = new List<StockItemAllergen>();
    public ICollection<AlternateCountUnit> AlternateUnits { get; set; } = new List<AlternateCountUnit>();
    public StockItemNutrition? NutritionalInfo { get; set; }
}
