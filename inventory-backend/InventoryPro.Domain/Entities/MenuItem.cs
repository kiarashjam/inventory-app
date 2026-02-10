using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class MenuItem
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public required string Name { get; set; }
    public string? Category { get; set; }
    public decimal SellingPrice { get; set; }
    public string? ExternalId { get; set; }
    public decimal? TheoreticalFoodCost { get; set; }
    public decimal? FoodCostPercent { get; set; }
    public MenuEngineeringCategory MenuEngineeringCategory { get; set; } = MenuEngineeringCategory.Unclassified;
    public decimal? ContributionMargin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
}
