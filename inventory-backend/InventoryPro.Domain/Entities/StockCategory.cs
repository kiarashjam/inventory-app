namespace InventoryPro.Domain.Entities;

public class StockCategory
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public required string Name { get; set; }
    public int DisplayOrder { get; set; }
    public int? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }

    public Organization Organization { get; set; } = null!;
    public StockCategory? ParentCategory { get; set; }
    public ICollection<StockCategory> SubCategories { get; set; } = new List<StockCategory>();
}
