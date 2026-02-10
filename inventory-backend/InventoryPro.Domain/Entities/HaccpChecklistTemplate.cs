using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class HaccpChecklistTemplate
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public HaccpFrequency Frequency { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<HaccpChecklistItem> Items { get; set; } = new List<HaccpChecklistItem>();
}
