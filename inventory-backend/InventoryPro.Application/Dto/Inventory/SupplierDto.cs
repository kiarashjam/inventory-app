namespace InventoryPro.Application.Dto.Inventory;

public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? PaymentTerms { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? PaymentTerms { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? PaymentTerms { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}
