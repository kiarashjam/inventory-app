namespace InventoryPro.Domain.Entities;

public class SaleRecord
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? PosConnectionId { get; set; }
    public string? ExternalOrderId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public PosConnection? PosConnection { get; set; }
    public ICollection<SaleRecordItem> Items { get; set; } = new List<SaleRecordItem>();
}
