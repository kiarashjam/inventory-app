using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class Organization
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoUrl { get; set; }
    public string Currency { get; set; } = "CHF";
    public string Timezone { get; set; } = "Europe/Zurich";
    public CostValuationMethod CostValuationMethod { get; set; } = CostValuationMethod.WeightedAverage;
    public decimal? DefaultFoodCostTargetPercent { get; set; } = 30;
    public decimal? DefaultBeverageCostTargetPercent { get; set; } = 20;
    public decimal VarianceAlertThresholdPercent { get; set; } = 3;
    public bool LowStockAlertEmail { get; set; } = true;
    public bool AutoDeductOnSale { get; set; } = true;
    public bool EnableHaccp { get; set; }
    public bool EnableAllergenTracking { get; set; }
    public bool EnablePrepLists { get; set; }
    public InventoryCountFrequency InventoryCountFrequency { get; set; } = InventoryCountFrequency.Weekly;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Free;
}
