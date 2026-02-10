using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class OrganizationUser
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public required string UserId { get; set; }
    public UserRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public AppUser AppUser { get; set; } = null!;
}
