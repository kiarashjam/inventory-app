namespace InventoryPro.Domain.Entities;

public class AllergenTag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
}
