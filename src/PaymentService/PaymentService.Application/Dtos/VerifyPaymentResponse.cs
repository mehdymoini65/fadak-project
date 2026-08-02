using PaymentService.Domain.Enums;

namespace PaymentService.Application.Dtos;

public sealed class VerifyPaymentResponse
{
    public bool IsSuccess { get; set; }
    public string Status { get; set; } = nameof(PaymentStatus.Pending);
    public decimal Amount { get; set; }
    public string? Rrn { get; set; }
    public string? ReservationNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}
