namespace InventoryPro.Domain.Entities;

public class StockItemNutrition
{
    public int Id { get; set; }
    public int StockItemId { get; set; }
    public decimal? CaloriesPer100g { get; set; }
    public decimal? ProteinPer100g { get; set; }
    public decimal? CarbsPer100g { get; set; }
    public decimal? FatPer100g { get; set; }
    public decimal? FiberPer100g { get; set; }
    public decimal? SodiumPer100g { get; set; }
    public decimal? SugarPer100g { get; set; }

    public StockItem StockItem { get; set; } = null!;
}
