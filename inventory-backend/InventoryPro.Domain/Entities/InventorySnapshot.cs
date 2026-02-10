using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class InventorySnapshot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public SnapshotType SnapshotType { get; set; }
    public decimal TotalValue { get; set; }
    public int TotalItems { get; set; }
    public DateTime CreatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<InventorySnapshotItem> Items { get; set; } = new List<InventorySnapshotItem>();
}
