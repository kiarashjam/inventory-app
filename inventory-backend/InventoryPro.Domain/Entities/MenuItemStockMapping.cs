using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class MenuItemStockMapping
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public int StockItemId { get; set; }
    public decimal QuantityRequired { get; set; }
    public UnitOfMeasurement UnitOfMeasurement { get; set; }
    public decimal WastePercentage { get; set; }
    public string? Notes { get; set; }

    public MenuItem MenuItem { get; set; } = null!;
    public StockItem StockItem { get; set; } = null!;
}
