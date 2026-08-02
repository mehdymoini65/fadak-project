using Microsoft.Extensions.Logging;
using PaymentService.Application.Contracts;
using Quartz;

namespace PaymentService.Infrastructure.Scheduling;

/// <summary>
/// Background job that expires pending transactions older than the configured
/// timeout. Runs on a fixed interval controlled by <c>PaymentExpiration</c> in
/// appsettings (default: every 30 seconds).
/// </summary>
[DisallowConcurrentExecution]
public sealed class TransactionExpirationJob : IJob
{
    public const string JobName = "TransactionExpirationJob";
    public const string GroupName = "payment";

    private readonly ITransactionExpirationService _expirationService;
    private readonly ILogger<TransactionExpirationJob> _logger;

    public TransactionExpirationJob(
        ITransactionExpirationService expirationService,
        ILogger<TransactionExpirationJob> logger)
    {
        _expirationService = expirationService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("TransactionExpirationJob started at {UtcNow}.", DateTimeOffset.UtcNow);

        try
        {
            var expiredCount = await _expirationService.ExpirePendingAsync(context.CancellationToken);
            _logger.LogInformation("TransactionExpirationJob finished. Expired {Count} transaction(s).", expiredCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TransactionExpirationJob failed.");
            throw new JobExecutionException(ex, false);
        }
    }
}
