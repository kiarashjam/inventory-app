namespace InventoryPro.Application.Dto.Inventory;

public class StockCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsActive { get; set; }
    public List<StockCategoryDto> SubCategories { get; set; } = new();
}

public class CreateStockCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int? ParentCategoryId { get; set; }
}

public class UpdateStockCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
}
