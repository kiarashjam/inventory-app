namespace InventoryPro.Application.Dto.Inventory;

public class WasteRecordDto
{
    public int Id { get; set; }
    public int StockItemId { get; set; }
    public string StockItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal TotalCost { get; set; }
    public string WasteReason { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string? RecordedBy { get; set; }
    public string? Notes { get; set; }
}

public class CreateWasteRecordDto
{
    public int StockItemId { get; set; }
    public decimal Quantity { get; set; }
    public int WasteReason { get; set; }
    public string? Notes { get; set; }
}

public class WasteSummaryDto
{
    public int TotalRecords { get; set; }
    public decimal TotalCost { get; set; }
    public List<WasteByReasonDto> ByReason { get; set; } = new();
}

public class WasteByReasonDto
{
    public string Reason { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalCost { get; set; }
}
