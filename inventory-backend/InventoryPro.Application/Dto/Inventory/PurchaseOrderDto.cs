using InventoryPro.Domain.Enums;

namespace InventoryPro.Application.Dto.Inventory;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PurchaseOrderDetailDto : PurchaseOrderDto
{
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
}

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int StockItemId { get; set; }
    public string StockItemName { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreatePurchaseOrderDto
{
    public int SupplierId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemDto
{
    public int StockItemId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdatePurchaseOrderDto
{
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}

public class GoodsReceivingDto
{
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public List<GoodsReceivingItemDto> Items { get; set; } = new();
}

public class GoodsReceivingItemDto
{
    public int PurchaseOrderItemId { get; set; }
    public int StockItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
