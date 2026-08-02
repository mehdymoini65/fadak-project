using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Events;

/// <summary>Published after the gateway reports a terminal payment result.</summary>
public sealed record PaymentProcessedEvent(
    Guid TransactionId,
    string Token,
    PaymentStatus Status,
    decimal Amount,
    string? Rrn,
    string ReservationNumber,
    string RedirectUrl,
    DateTimeOffset OccurredAt);
