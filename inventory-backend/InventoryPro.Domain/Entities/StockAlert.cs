using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class StockAlert
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? StockItemId { get; set; }
    public AlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsDismissed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ReadBy { get; set; }

    public Organization Organization { get; set; } = null!;
    public StockItem? StockItem { get; set; }
}
