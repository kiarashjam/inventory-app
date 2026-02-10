namespace InventoryPro.Domain.Entities;

public class HaccpChecklistItem
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsCritical { get; set; }

    public HaccpChecklistTemplate Template { get; set; } = null!;
}
