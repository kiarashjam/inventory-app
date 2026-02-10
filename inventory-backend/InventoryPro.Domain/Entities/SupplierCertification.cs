namespace InventoryPro.Domain.Entities;

public class SupplierCertification
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string CertificationType { get; set; } = string.Empty;
    public string? CertificationNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? DocumentUrl { get; set; }
    public bool IsActive { get; set; }

    public Supplier Supplier { get; set; } = null!;
}
