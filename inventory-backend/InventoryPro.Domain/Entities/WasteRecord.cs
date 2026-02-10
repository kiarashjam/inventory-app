using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class WasteRecord
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int StockItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal TotalCost { get; set; }
    public WasteReason WasteReason { get; set; }
    public DateTime RecordedAt { get; set; }
    public string? RecordedBy { get; set; }
    public string? Notes { get; set; }

    public Organization Organization { get; set; } = null!;
    public StockItem StockItem { get; set; } = null!;
}
