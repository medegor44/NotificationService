using System.Collections.Concurrent;
using Domain;
using NotificationExchange.Abstractions;

namespace NotificationExchange;

public class Exchange : IExchangeConnector, IExchangeSender
{
    private readonly ConcurrentDictionary<UserId, UserSessions> _connections = new();
    
    public ISessionQueue ConnectUserSession(UserId userId)
    {
        if (!_connections.TryGetValue(userId, out var sessions))
        {
            sessions = new UserSessions();
            _connections.TryAdd(userId, sessions);
        }

        return sessions.GetSession(Guid.NewGuid());
    }

    public void DisconnectUserSession(UserId userId, ISessionQueue sessionQueue)
    {
        if (!_connections.TryGetValue(userId, out var sessions)) 
            return;
        
        sessions.RemoveSession(sessionQueue);
        
        if (sessions.Empty)
            _connections.TryRemove(userId, out _);
    }

    public void Send(Message message)
    {
        if (!_connections.TryGetValue(message.UserId, out var sessions))
            return;
        
        sessions.PushMessage(message);
    }
}