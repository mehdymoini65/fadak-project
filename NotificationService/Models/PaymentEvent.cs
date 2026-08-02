namespace NotificationService.Models;

public sealed class PaymentEvent
{
    public Guid TransactionId { get; set; }
    public string Token { get; set; } = string.Empty;
    public int Status { get; set; }
    public decimal Amount { get; set; }
    public string? Rrn { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}
