using Domain;

namespace NotificationExchange.Abstractions;

public interface IExchangeSender
{
    void Send(Message message);
}