namespace InventoryPro.Application.Dto.Inventory;

public class MenuItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal SellingPrice { get; set; }
    public string? ExternalId { get; set; }
    public decimal? TheoreticalFoodCost { get; set; }
    public decimal? FoodCostPercent { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal SellingPrice { get; set; }
    public string? ExternalId { get; set; }
}

public class UpdateMenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal SellingPrice { get; set; }
    public string? ExternalId { get; set; }
    public bool? IsActive { get; set; }
}
