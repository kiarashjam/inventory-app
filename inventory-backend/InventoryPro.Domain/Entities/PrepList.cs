using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class PrepList
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public DateTime PrepDate { get; set; }
    public PrepListStatus Status { get; set; } = PrepListStatus.Generated;
    public string? GeneratedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<PrepListItem> Items { get; set; } = new List<PrepListItem>();
}
