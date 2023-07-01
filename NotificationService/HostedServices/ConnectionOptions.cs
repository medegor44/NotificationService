namespace NotificationService.HostedServices;

public class ConnectionOptions
{
    public const string Section = "RabbitMq:Connection";
    public string HostName { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}