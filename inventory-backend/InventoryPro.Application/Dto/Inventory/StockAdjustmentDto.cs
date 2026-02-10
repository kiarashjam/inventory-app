namespace InventoryPro.Application.Dto.Inventory;

public class StockAdjustmentDto
{
    public decimal Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
