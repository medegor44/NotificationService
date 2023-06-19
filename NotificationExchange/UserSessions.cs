using System.Collections.Concurrent;
using Domain;
using NotificationExchange.Abstractions;

namespace NotificationExchange;

internal class UserSessions
{
    private readonly ConcurrentDictionary<Guid, SessionQueue> _queues = new();

    public bool Empty => _queues.IsEmpty;
    
    public void PushMessage(Message message)
    {
        foreach (var sessionQueue in _queues.Values)
            sessionQueue.PushMessage(message);
    }

    public void RemoveSession(ISessionQueue sessionQueue)
    {
        _queues.TryRemove(sessionQueue.SessionId, out _);
    }

    public SessionQueue GetSession(Guid sessionId)
    {
        if (!_queues.TryGetValue(sessionId, out var session))
        {
           session = new SessionQueue(sessionId);
            _queues.TryAdd(sessionId, session);
        }

        return session;
    }
}