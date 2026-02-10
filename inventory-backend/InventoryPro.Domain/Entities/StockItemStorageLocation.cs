namespace InventoryPro.Domain.Entities;

public class StockItemStorageLocation
{
    public int Id { get; set; }
    public int StockItemId { get; set; }
    public int StorageLocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? ShelfPosition { get; set; }

    public StockItem StockItem { get; set; } = null!;
    public StorageLocation StorageLocation { get; set; } = null!;
}
