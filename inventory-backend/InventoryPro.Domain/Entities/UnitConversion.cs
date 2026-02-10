using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class UnitConversion
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public UnitOfMeasurement FromUnit { get; set; }
    public UnitOfMeasurement ToUnit { get; set; }
    public decimal ConversionFactor { get; set; }

    public Organization Organization { get; set; } = null!;
}
