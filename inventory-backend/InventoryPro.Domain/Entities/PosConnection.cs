namespace InventoryPro.Domain.Entities;

public class PosConnection
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string PosSystemName { get; set; } = string.Empty;
    public string ApiKeyHash { get; set; } = string.Empty;
    public string? WebhookSecret { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
}
