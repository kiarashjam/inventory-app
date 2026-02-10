namespace InventoryPro.Application.Dto.Inventory;

public class SupplierStockItemDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int StockItemId { get; set; }
    public string StockItemName { get; set; } = string.Empty;
    public string? SupplierSKU { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? MinimumOrderQuantity { get; set; }
    public bool IsPreferred { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

public class CreateSupplierStockItemDto
{
    public int SupplierId { get; set; }
    public int StockItemId { get; set; }
    public string? SupplierSKU { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? MinimumOrderQuantity { get; set; }
    public bool IsPreferred { get; set; }
}

public class UpdateSupplierStockItemDto
{
    public string? SupplierSKU { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? MinimumOrderQuantity { get; set; }
    public bool IsPreferred { get; set; }
}
