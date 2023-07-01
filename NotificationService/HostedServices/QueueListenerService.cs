using System.Text;
using System.Text.Json;
using Domain;
using Microsoft.Extensions.Options;
using NotificationExchange.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.HostedServices;

public class QueueListenerService : BackgroundService
{
    private readonly ILogger<QueueListenerService> _logger;
    private readonly IExchangeSender _exchange;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private QueueDeclareOk _queueMetadata;

    private const string ExchangeName = "NotificationsExchange";
    private const string PostNotificationRoutingKey = nameof(PostNotificationRoutingKey);

    public QueueListenerService(
        ILogger<QueueListenerService> logger, 
        IExchangeSender exchange, 
        IOptions<ConnectionOptions> connectionOptions,
        IOptions<FeedNotificationsOptions> feedNotificationsOptions)
    {
        _logger = logger;
        _exchange = exchange;

        _connection = new ConnectionFactory()
        {
            HostName = connectionOptions.Value.HostName,
            UserName = connectionOptions.Value.UserName,
            Password = connectionOptions.Value.Password,
            Port = connectionOptions.Value.Port
        }.CreateConnection();
        
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(ExchangeName, type: ExchangeType.Direct);
        
        _queueMetadata = _channel.QueueDeclare(
            queue: feedNotificationsOptions.Value.Name,
            durable: feedNotificationsOptions.Value.Durable,
            exclusive: feedNotificationsOptions.Value.Exclusive,
            autoDelete: feedNotificationsOptions.Value.AutoDelete
        );
        
        _channel.QueueBind(_queueMetadata.QueueName, ExchangeName, PostNotificationRoutingKey);
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += ConsumerOnReceived;

        _channel.BasicConsume(
            queue: _queueMetadata.QueueName, 
            autoAck: false, 
            consumer: consumer);
        
        return Task.CompletedTask;
    }

    private void ConsumerOnReceived(object? _, BasicDeliverEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.Body.Span);
        
        _logger.LogInformation("Received message: {message}", message);

        var notification = JsonSerializer.Deserialize<PostCreatedNotification>(message);
        
        if (notification is not null)
            _exchange.Send(new(new UserId(notification.RecipientId), message));

        _channel.BasicAck(e.DeliveryTag, false);
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        
        base.Dispose();
    }
}