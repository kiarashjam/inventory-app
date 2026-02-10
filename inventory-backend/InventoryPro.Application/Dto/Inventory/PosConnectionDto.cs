namespace InventoryPro.Application.Dto.Inventory;

public class PosConnectionDto
{
    public int Id { get; set; }
    public string PosSystemName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    // Show only last 8 chars of the key hash for display
    public string ApiKeyPreview { get; set; } = string.Empty;
}

public class PosConnectionCreatedDto
{
    public int Id { get; set; }
    public string PosSystemName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    // The FULL API key - only shown once on creation
    public string ApiKey { get; set; } = string.Empty;
    public string? WebhookSecret { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePosConnectionDto
{
    public string PosSystemName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
