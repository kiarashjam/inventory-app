using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class HaccpChecklistLog
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int OrganizationId { get; set; }
    public DateTime CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public HaccpLogStatus Status { get; set; }
    public string? Notes { get; set; }

    public HaccpChecklistTemplate Template { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public ICollection<HaccpChecklistLogItem> Items { get; set; } = new List<HaccpChecklistLogItem>();
}
