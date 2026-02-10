namespace InventoryPro.Application.Dto.Inventory;

public class OrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Currency { get; set; } = "CHF";
    public string Timezone { get; set; } = "Europe/Zurich";
    public decimal? DefaultFoodCostTargetPercent { get; set; }
    public decimal? DefaultBeverageCostTargetPercent { get; set; }
    public decimal VarianceAlertThresholdPercent { get; set; }
    public bool LowStockAlertEmail { get; set; }
    public bool AutoDeductOnSale { get; set; }
    public bool EnableHaccp { get; set; }
    public bool EnableAllergenTracking { get; set; }
    public bool EnablePrepLists { get; set; }
    public string SubscriptionPlan { get; set; } = "Free";
    public DateTime CreatedAt { get; set; }
}

public class UpdateOrganizationDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Currency { get; set; }
    public string? Timezone { get; set; }
    public decimal? DefaultFoodCostTargetPercent { get; set; }
    public decimal? DefaultBeverageCostTargetPercent { get; set; }
    public decimal? VarianceAlertThresholdPercent { get; set; }
    public bool? LowStockAlertEmail { get; set; }
    public bool? AutoDeductOnSale { get; set; }
    public bool? EnableHaccp { get; set; }
    public bool? EnableAllergenTracking { get; set; }
    public bool? EnablePrepLists { get; set; }
}
