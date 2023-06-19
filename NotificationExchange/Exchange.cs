using System.Collections.Concurrent;
using Domain;
using NotificationExchange.Abstractions;

namespace NotificationExchange;

public class Exchange : IExchangeConnector, IExchangeSender
{
    private readonly ConcurrentDictionary<UserName, UserSessions> _connections = new();
    
    public ISessionQueue ConnectUserSession(UserName userName)
    {
        if (!_connections.TryGetValue(userName, out var sessions))
        {
            sessions = new UserSessions();
            _connections.TryAdd(userName, sessions);
        }

        return sessions.GetSession(Guid.NewGuid());
    }

    public void DisconnectUserSession(UserName userName, ISessionQueue sessionQueue)
    {
        if (!_connections.TryGetValue(userName, out var sessions)) 
            return;
        
        sessions.RemoveSession(sessionQueue);
        
        if (sessions.Empty)
            _connections.TryRemove(userName, out _);
    }

    public void Send(Message message)
    {
        if (!_connections.TryGetValue(message.UserName, out var sessions))
            return;
        
        sessions.PushMessage(message);
    }
}