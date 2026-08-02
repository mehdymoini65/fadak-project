using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Abstractions;

/// <summary>
/// Persistence contract for payment transactions.
/// All state transitions are performed as atomic conditional updates so that
/// concurrent callers (gateway callback, verify, expiration job) cannot
/// overwrite each other.
/// </summary>
public interface ITransactionRepository
{
    Task<Transaction?> GetByTokenAsync(string token, CancellationToken cancellationToken);

    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically moves a transaction from <paramref name="fromStatus"/> to
    /// <paramref name="toStatus"/>. Returns the current transaction and whether
    /// the transition actually happened (false when the transaction was already
    /// moved by someone else, or does not exist).
    /// </summary>
    Task<TransactionTransitionResult> TryTransitionAsync(
        string token,
        PaymentStatus fromStatus,
        PaymentStatus toStatus,
        string? rrn,
        CancellationToken cancellationToken);

    /// <summary>
    /// Atomically expires a pending transaction created before <paramref name="cutoff"/>.
    /// Returns true only if this call actually performed the transition.
    /// </summary>
    Task<bool> TryExpireAsync(string token, DateTime cutoff, CancellationToken cancellationToken);

    Task<IReadOnlyList<Transaction>> GetPendingOlderThanAsync(DateTime before, CancellationToken cancellationToken);

    Task UpdateAppCodeAsync(string token, string? appCode, CancellationToken cancellationToken);
}

public sealed record TransactionTransitionResult(bool Changed, Transaction? Transaction);
