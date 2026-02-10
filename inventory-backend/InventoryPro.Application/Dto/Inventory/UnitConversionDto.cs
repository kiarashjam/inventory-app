namespace InventoryPro.Application.Dto.Inventory;

public class UnitConversionDto
{
    public int Id { get; set; }
    public int FromUnit { get; set; }
    public int ToUnit { get; set; }
    public decimal ConversionFactor { get; set; }
}

public class CreateUnitConversionDto
{
    public int FromUnit { get; set; }
    public int ToUnit { get; set; }
    public decimal ConversionFactor { get; set; }
}
