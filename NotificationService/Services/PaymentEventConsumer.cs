using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.Models;
using NotificationService.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Services;

public sealed class PaymentEventConsumer : BackgroundService
{
    private readonly RabbitMqOptions _rabbit;
    private readonly CallbackOptions _callback;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PaymentEventConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public PaymentEventConsumer(
        IOptions<RabbitMqOptions> rabbit,
        IOptions<CallbackOptions> callback,
        IHttpClientFactory httpClientFactory,
        ILogger<PaymentEventConsumer> logger)
    {
        _rabbit = rabbit.Value;
        _callback = callback.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                StartConsumer(stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ consumer failed; retrying in 5 seconds.");
                Cleanup();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private void StartConsumer(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbit.HostName,
            Port = _rabbit.Port,
            UserName = _rabbit.UserName,
            Password = _rabbit.Password,
            VirtualHost = _rabbit.VirtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_rabbit.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        _channel.BasicQos(0, 10, false);

        BindAndConsume("PaymentProcessedEvent", stoppingToken);
        BindAndConsume("PaymentExpiredEvent", stoppingToken);
        _logger.LogInformation("Notification consumer connected to RabbitMQ at {Host}:{Port}.", _rabbit.HostName, _rabbit.Port);
    }

    private void BindAndConsume(string eventName, CancellationToken stoppingToken)
    {
        var queue = $"{_rabbit.ExchangeName}.{eventName}";
        var routingKey = $"{_rabbit.RoutingKey}.{eventName}";
        _channel!.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue, _rabbit.ExchangeName, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, args) =>
        {
            try
            {
                var paymentEvent = JsonSerializer.Deserialize<PaymentEvent>(args.Body.Span, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new JsonException("Payment event body was empty.");

                _logger.LogInformation(
                    "Received {EventType}: Token={Token}, Status={Status}, Amount={Amount}, RRN={Rrn}.",
                    args.BasicProperties.Type ?? eventName, paymentEvent.Token, paymentEvent.Status, paymentEvent.Amount, paymentEvent.Rrn);

                await SendCallbackAsync(paymentEvent, stoppingToken);
                _channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process {EventType}; message will be dead-lettered/discarded.", eventName);
                _channel.BasicNack(args.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume(queue, autoAck: false, consumer);
    }

    private async Task SendCallbackAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(paymentEvent.RedirectUrl, UriKind.Absolute, out var callbackUri) ||
            (callbackUri.Scheme != Uri.UriSchemeHttp && callbackUri.Scheme != Uri.UriSchemeHttps))
        {
            _logger.LogWarning("Callback skipped for token {Token}: RedirectUrl is invalid.", paymentEvent.Token);
            return;
        }

        Exception? lastError = null;
        for (var attempt = 1; attempt <= Math.Max(1, _callback.RetryCount); attempt++)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("callback");
                using var response = await client.PostAsJsonAsync(callbackUri, paymentEvent, cancellationToken);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Callback sent for token {Token} to {Url}.", paymentEvent.Token, callbackUri);
                return;
            }
            catch (Exception ex) when (attempt < Math.Max(1, _callback.RetryCount))
            {
                lastError = ex;
                await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        throw new HttpRequestException($"Callback failed after {_callback.RetryCount} attempt(s).", lastError);
    }

    private void Cleanup()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _channel = null;
        _connection = null;
    }

    public override void Dispose()
    {
        Cleanup();
        base.Dispose();
    }
}
