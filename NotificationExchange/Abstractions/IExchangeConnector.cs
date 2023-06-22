using Domain;

namespace NotificationExchange.Abstractions;

public interface IExchangeConnector
{
    ISessionQueue ConnectUserSession(UserId userId);
    void DisconnectUserSession(UserId userId, ISessionQueue sessionQueue);
}