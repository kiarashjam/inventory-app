namespace InventoryPro.Domain.Entities;

public class ExternalProductMapping
{
    public int Id { get; set; }
    public int PosConnectionId { get; set; }
    public string ExternalProductId { get; set; } = string.Empty;
    public string? ExternalProductName { get; set; }
    public int MenuItemId { get; set; }
    public decimal QuantityMultiplier { get; set; } = 1;
    public bool IsActive { get; set; }

    public PosConnection PosConnection { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
}
