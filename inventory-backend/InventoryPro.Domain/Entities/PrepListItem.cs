namespace InventoryPro.Domain.Entities;

public class PrepListItem
{
    public int Id { get; set; }
    public int PrepListId { get; set; }
    public int StockItemId { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal OnHandQuantity { get; set; }
    public decimal PrepQuantity { get; set; }
    public decimal? ActualPrepQuantity { get; set; }
    public string? AssignedTo { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }

    public PrepList PrepList { get; set; } = null!;
    public StockItem StockItem { get; set; } = null!;
}
