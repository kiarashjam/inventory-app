namespace InventoryPro.Domain.Entities;

public class SaleRecordItem
{
    public int Id { get; set; }
    public int SaleRecordId { get; set; }
    public int? MenuItemId { get; set; }
    public string? ExternalProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }

    public SaleRecord SaleRecord { get; set; } = null!;
    public MenuItem? MenuItem { get; set; }
}
