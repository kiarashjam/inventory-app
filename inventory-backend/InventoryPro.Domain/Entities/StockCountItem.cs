namespace InventoryPro.Domain.Entities;

public class StockCountItem
{
    public int Id { get; set; }
    public int StockCountId { get; set; }
    public int StockItemId { get; set; }
    public decimal ExpectedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Variance { get; set; }
    public decimal VarianceValue { get; set; }
    public string? Notes { get; set; }
    public string? CountedBy { get; set; }
    public DateTime CountedAt { get; set; }

    public StockCount StockCount { get; set; } = null!;
    public StockItem StockItem { get; set; } = null!;
}
