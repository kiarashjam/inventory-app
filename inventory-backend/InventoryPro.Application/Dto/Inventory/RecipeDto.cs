using InventoryPro.Domain.Enums;

namespace InventoryPro.Application.Dto.Inventory;

public class RecipeDto
{
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal FoodCostPercent { get; set; }
    public List<RecipeIngredientDto> Ingredients { get; set; } = new();
}

public class RecipeIngredientDto
{
    public int MappingId { get; set; }
    public int StockItemId { get; set; }
    public string StockItemName { get; set; } = string.Empty;
    public decimal QuantityRequired { get; set; }
    public UnitOfMeasurement UnitOfMeasurement { get; set; }
    public decimal WastePercentage { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal SubTotal { get; set; }
    public string? Notes { get; set; }
}

public class RecipeMappingDto
{
    public int StockItemId { get; set; }
    public decimal QuantityRequired { get; set; }
    public UnitOfMeasurement UnitOfMeasurement { get; set; }
    public decimal WastePercentage { get; set; }
    public string? Notes { get; set; }
}
