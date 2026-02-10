using InventoryPro.Domain.Enums;

namespace InventoryPro.Application.Dto.Inventory;

public class StockMovementDto
{
    public int Id { get; set; }
    public int StockItemId { get; set; }
    public string StockItemName { get; set; } = string.Empty;
    public MovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public decimal? CostPerUnit { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? Notes { get; set; }
}
