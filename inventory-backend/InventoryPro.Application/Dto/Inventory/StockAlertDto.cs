using InventoryPro.Domain.Enums;

namespace InventoryPro.Application.Dto.Inventory;

public class StockAlertDto
{
    public int Id { get; set; }
    public int? StockItemId { get; set; }
    public string? StockItemName { get; set; }
    public AlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
