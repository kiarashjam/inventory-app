namespace InventoryPro.Domain.Entities;

public class InventorySnapshotItem
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    public int StockItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal TotalValue { get; set; }

    public InventorySnapshot Snapshot { get; set; } = null!;
    public StockItem StockItem { get; set; } = null!;
}
