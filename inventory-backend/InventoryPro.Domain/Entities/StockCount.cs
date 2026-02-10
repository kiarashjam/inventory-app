using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class StockCount
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public DateTime CountDate { get; set; }
    public StockCountStatus Status { get; set; } = StockCountStatus.InProgress;
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<StockCountItem> Items { get; set; } = new List<StockCountItem>();
}
