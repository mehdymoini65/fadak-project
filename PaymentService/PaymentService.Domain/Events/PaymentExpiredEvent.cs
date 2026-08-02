using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Events;

/// <summary>Published when a pending transaction expires.</summary>
public sealed record PaymentExpiredEvent(
    Guid TransactionId,
    string Token,
    PaymentStatus Status,
    decimal Amount,
    string ReservationNumber,
    string RedirectUrl,
    DateTimeOffset OccurredAt);
