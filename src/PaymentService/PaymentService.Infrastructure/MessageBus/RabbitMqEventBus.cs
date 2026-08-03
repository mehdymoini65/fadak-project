using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.Options;
using PaymentService.Domain.Abstractions;
using RabbitMQ.Client;

namespace PaymentService.Infrastructure.MessageBus;

/// <summary>
/// Publishes integration events to a RabbitMQ topic exchange.
/// A durable queue is declared (and bound) for every event type so consumers can subscribe reliably.
/// The connection is established lazily so the service can start even when RabbitMQ is down.
/// </summary>
public sealed class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly SemaphoreSlim _sync = new(1, 1);
    private readonly HashSet<string> _declaredQueues = new(StringComparer.Ordinal);

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqEventBus(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventBus> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class
    {
        await EnsureConnectedAsync(cancellationToken);

        var eventName = typeof(TEvent).Name;
        var routingKey = $"{_options.RoutingKey}.{eventName}";

        EnsureQueue(eventName, routingKey);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event);

        var properties = _channel!.CreateBasicProperties();
        properties.Type = eventName;
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2; // persistent

        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _channel!.ConfirmSelect();
                _channel.BasicPublish(
                    exchange: _options.ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
                _channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
                break;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "RabbitMQ publish attempt {Attempt}/{MaxAttempts} failed for {EventType}.", attempt, maxAttempts, eventName);
                ResetConnection();
                await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
                await EnsureConnectedAsync(cancellationToken);
                EnsureQueue(eventName, routingKey);
            }
        }

        _logger.LogInformation(
            "Published {EventType} to {Exchange}/{RoutingKey}.",
            eventName,
            _options.ExchangeName,
            routingKey);
    }

    private void EnsureQueue(string eventName, string bindingKey)
    {
        if (!_declaredQueues.Add(eventName))
        {
            return;
        }

        var queueName = $"{_options.ExchangeName}.{eventName}";

        _channel!.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, _options.ExchangeName, bindingKey);

        _logger.LogInformation("Declared queue {Queue} bound to {Exchange} with key {BindingKey}.",
            queueName, _options.ExchangeName, bindingKey);
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return;
        }

        await _sync.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true })
            {
                return;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

            _declaredQueues.Clear();

            _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}.", _options.HostName, _options.Port);
        }
        finally
        {
            _sync.Release();
        }
    }

    private void ResetConnection()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _channel = null;
        _connection = null;
        _declaredQueues.Clear();
    }

    public void Dispose()
    {
        ResetConnection();
        _sync.Dispose();
    }
}
