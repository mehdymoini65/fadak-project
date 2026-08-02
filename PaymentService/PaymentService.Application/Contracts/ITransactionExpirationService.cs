namespace PaymentService.Application.Contracts;

/// <summary>
/// Expires pending transactions older than the configured timeout.
/// Used by the background expiration job.
/// </summary>
public interface ITransactionExpirationService
{
    Task<int> ExpirePendingAsync(CancellationToken cancellationToken);
}
