using System.Collections.Concurrent;
using Domain;
using NotificationExchange.Abstractions;

namespace NotificationExchange;

public class SessionQueue : ISessionQueue
{
    public Guid SessionId { get; }
    private readonly ConcurrentQueue<Message> _queue = new();

    public SessionQueue(Guid sessionId)
    {
        SessionId = sessionId;
    }
    
    public IEnumerable<Message> Messages(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            while (_queue.TryDequeue(out var message))
                yield return message;
        }
    }

    internal void PushMessage(Message message)
    {
        _queue.Enqueue(message);
    }
}