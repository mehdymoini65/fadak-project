namespace PaymentService.Domain.Abstractions;

/// <summary>
/// Abstraction over the message bus (RabbitMQ) used to publish integration events.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class;
}
