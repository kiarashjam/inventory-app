namespace InventoryPro.Domain.Entities;

public class SupplierStockItem
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int StockItemId { get; set; }
    public string? SupplierSKU { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? MinimumOrderQuantity { get; set; }
    public bool IsPreferred { get; set; }
    public DateTime? LastOrderDate { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public StockItem StockItem { get; set; } = null!;
}
