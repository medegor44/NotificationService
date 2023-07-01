namespace NotificationService.HostedServices;

public class FeedNotificationsOptions
{
    public const string Section = "RabbitMq:FeedNotifications";
    public string Name { get; set; }
    public bool Durable { get; set; }
    public bool Exclusive { get; set; }
    public bool AutoDelete { get; set; }
}