using Domain;

namespace NotificationExchange.Abstractions;

public interface ISessionQueue
{
    Guid SessionId { get; }
    IEnumerable<Message> Messages(CancellationToken cancellationToken);
}