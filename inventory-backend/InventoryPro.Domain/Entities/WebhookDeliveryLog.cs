namespace InventoryPro.Domain.Entities;

public class WebhookDeliveryLog
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime DeliveredAt { get; set; }
    public bool IsSuccess { get; set; }

    public WebhookSubscription Subscription { get; set; } = null!;
}
