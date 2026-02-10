namespace InventoryPro.Application.Dto.Inventory;

public class StockCountDto
{
    public int Id { get; set; }
    public DateTime CountDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalVarianceValue { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StockCountDetailDto : StockCountDto
{
    public List<StockCountItemResultDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class StockCountItemResultDto
{
    public int Id { get; set; }
    public int StockItemId { get; set; }
    public string StockItemName { get; set; } = string.Empty;
    public decimal ExpectedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Variance { get; set; }
    public decimal VarianceValue { get; set; }
    public string? CountedBy { get; set; }
    public DateTime CountedAt { get; set; }
}

public class CreateStockCountDto
{
    public string? Notes { get; set; }
    public int? CategoryId { get; set; }
    public int? StorageLocationId { get; set; }
}

public class StockCountItemDto
{
    public int StockItemId { get; set; }
    public decimal ActualQuantity { get; set; }
    public string? Notes { get; set; }
}
