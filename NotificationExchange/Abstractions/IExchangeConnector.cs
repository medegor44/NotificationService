using Domain;

namespace NotificationExchange.Abstractions;

public interface IExchangeConnector
{
    ISessionQueue ConnectUserSession(UserName userName);
    void DisconnectUserSession(UserName userName, ISessionQueue sessionQueue);
}