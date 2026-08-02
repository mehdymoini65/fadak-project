using PaymentService.Domain.Enums;

namespace PaymentService.Application.Dtos;

public sealed class TransactionInfoResponse
{
    public string Token { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? Rrn { get; set; }
    public string? AppCode { get; set; }
    public string TerminalNo { get; set; } = string.Empty;
    public string ReservationNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
