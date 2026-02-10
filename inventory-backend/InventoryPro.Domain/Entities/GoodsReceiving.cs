using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class GoodsReceiving
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int OrganizationId { get; set; }
    public DateTime ReceivingDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal? InvoiceAmount { get; set; }
    public string? Notes { get; set; }
    public GoodsReceivingStatus Status { get; set; } = GoodsReceivingStatus.Pending;
    public DateTime CreatedAt { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public ICollection<GoodsReceivingItem> Items { get; set; } = new List<GoodsReceivingItem>();
}
