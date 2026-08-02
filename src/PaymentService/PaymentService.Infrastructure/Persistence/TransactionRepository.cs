using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Abstractions;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.Persistence;

/// <summary>
/// EF-backed repository. State transitions use conditional UPDATE statements so
/// concurrent transitions (gateway callback, verify, expiration job) are atomic
/// and idempotent at the database level.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly PaymentDbContext _db;

    public TransactionRepository(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task<Transaction?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _db.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new DuplicateTokenException(transaction.Token, ex);
        }
    }

    public async Task<TransactionTransitionResult> TryTransitionAsync(
        string token,
        PaymentStatus fromStatus,
        PaymentStatus toStatus,
        string? rrn,
        CancellationToken cancellationToken)
    {
        var affected = await _db.Transactions
            .Where(t => t.Token == token && t.Status == fromStatus)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.Status, toStatus)
                    .SetProperty(t => t.Rrn, rrn)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow),
                cancellationToken);

        var transaction = await GetByTokenAsync(token, cancellationToken);

        if (transaction is null)
        {
            return new TransactionTransitionResult(false, null);
        }

        return new TransactionTransitionResult(affected == 1, transaction);
    }

    public async Task<bool> TryExpireAsync(string token, DateTime cutoff, CancellationToken cancellationToken)
    {
        var affected = await _db.Transactions
            .Where(t => t.Token == token && t.Status == PaymentStatus.Pending && t.CreatedAt < cutoff)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.Status, PaymentStatus.Expired)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow),
                cancellationToken);

        return affected == 1;
    }

    public async Task<IReadOnlyList<Transaction>> GetPendingOlderThanAsync(DateTime before, CancellationToken cancellationToken)
    {
        return await _db.Transactions
            .AsNoTracking()
            .Where(t => t.Status == PaymentStatus.Pending && t.CreatedAt < before)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAppCodeAsync(string token, string? appCode, CancellationToken cancellationToken)
    {
        await _db.Transactions
            .Where(t => t.Token == token)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.AppCode, appCode)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message?.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true;
    }
}
