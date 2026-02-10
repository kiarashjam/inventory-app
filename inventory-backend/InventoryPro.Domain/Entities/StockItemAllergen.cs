using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class StockItemAllergen
{
    public int Id { get; set; }
    public int StockItemId { get; set; }
    public int AllergenTagId { get; set; }
    public AllergenSeverity Severity { get; set; }
    public string? Notes { get; set; }

    public StockItem StockItem { get; set; } = null!;
    public AllergenTag AllergenTag { get; set; } = null!;
}
