using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.Contracts;
using PaymentService.Application.Options;
using PaymentService.Domain.Abstractions;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Events;

namespace PaymentService.Application.Services;

public sealed class TransactionExpirationService : ITransactionExpirationService
{
    private readonly ITransactionRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly PaymentExpirationOptions _options;
    private readonly ILogger<TransactionExpirationService> _logger;

    public TransactionExpirationService(
        ITransactionRepository repository,
        IEventBus eventBus,
        IOptions<PaymentExpirationOptions> options,
        ILogger<TransactionExpirationService> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> ExpirePendingAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-_options.TimeoutMinutes);
        var candidates = await _repository.GetPendingOlderThanAsync(cutoff, cancellationToken);

        var expiredCount = 0;

        foreach (var transaction in candidates)
        {
            if (await _repository.TryExpireAsync(transaction.Token, cutoff, cancellationToken))
            {
                expiredCount++;

                await PublishAsync(
                    new PaymentExpiredEvent(
                        transaction.Id,
                        transaction.Token,
                        PaymentStatus.Expired,
                        transaction.Amount,
                        transaction.ReservationNumber,
                        transaction.RedirectUrl,
                        DateTimeOffset.UtcNow),
                    cancellationToken);
            }
        }

        if (expiredCount > 0)
        {
            _logger.LogInformation("Expired {Count} pending transaction(s).", expiredCount);
        }

        return expiredCount;
    }

    private async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class
    {
        try
        {
            await _eventBus.PublishAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} while expiring transactions.", typeof(TEvent).Name);
        }
    }
}
