namespace InventoryPro.Domain.Entities;

public class GoodsReceivingItem
{
    public int Id { get; set; }
    public int GoodsReceivingId { get; set; }
    public int PurchaseOrderItemId { get; set; }
    public int StockItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Notes { get; set; }

    public GoodsReceiving GoodsReceiving { get; set; } = null!;
    public PurchaseOrderItem PurchaseOrderItem { get; set; } = null!;
    public StockItem StockItem { get; set; } = null!;
}
