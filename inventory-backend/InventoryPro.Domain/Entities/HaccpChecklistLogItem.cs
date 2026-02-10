namespace InventoryPro.Domain.Entities;

public class HaccpChecklistLogItem
{
    public int Id { get; set; }
    public int LogId { get; set; }
    public int ChecklistItemId { get; set; }
    public string? ActualValue { get; set; }
    public bool IsPassed { get; set; }
    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }

    public HaccpChecklistLog Log { get; set; } = null!;
    public HaccpChecklistItem ChecklistItem { get; set; } = null!;
}
