using System.Text;
using System.Text.Json;
using Domain;
using NotificationExchange.Abstractions;
using NotificationService.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.HostedServices;

public class QueueListenerService : BackgroundService
{
    private readonly ILogger<QueueListenerService> _logger;
    private readonly IExchangeSender _exchange;
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public QueueListenerService(ILogger<QueueListenerService> logger, IExchangeSender exchange)
    {
        _logger = logger;
        _exchange = exchange;

        _connection = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "rmuser",
            Password = "rmpassword"
        }.CreateConnection();
        
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(
            queue: "FeedNotifications",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += ConsumerOnReceived;

        _channel.BasicConsume(
            queue: "FeedNotifications", 
            autoAck: false, 
            consumer: consumer);
        
        return Task.CompletedTask;
    }

    private void ConsumerOnReceived(object? _, BasicDeliverEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.Body.Span);
        
        _logger.LogInformation("Received message: {message}", message);

        var newPostMessage = JsonSerializer.Deserialize<NewPostMessage>(message);
        
        _exchange.Send(new(
            new UserName(newPostMessage.UserName), 
            $"New post from {newPostMessage.AuthorName}"));

        _channel.BasicAck(e.DeliveryTag, false);
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        
        base.Dispose();
    }
}