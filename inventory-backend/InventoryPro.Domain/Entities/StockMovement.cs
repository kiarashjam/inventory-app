using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class StockMovement
{
    public int Id { get; set; }
    public int StockItemId { get; set; }
    public int OrganizationId { get; set; }
    public MovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public decimal? CostPerUnit { get; set; }
    public decimal? TotalCost { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? Notes { get; set; }

    public StockItem StockItem { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}
