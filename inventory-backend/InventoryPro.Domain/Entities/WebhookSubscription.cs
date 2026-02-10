using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class WebhookSubscription
{
    public int Id { get; set; }
    public int PosConnectionId { get; set; }
    public WebhookEventType EventType { get; set; }
    public string TargetUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public PosConnection PosConnection { get; set; } = null!;
}
