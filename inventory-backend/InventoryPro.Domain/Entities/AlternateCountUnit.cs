namespace InventoryPro.Domain.Entities;

public class AlternateCountUnit
{
    public int Id { get; set; }
    public int StockItemId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal ConversionFactor { get; set; }
    public string? Barcode { get; set; }

    public StockItem StockItem { get; set; } = null!;
}
