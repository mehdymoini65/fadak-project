namespace GatewayService.Domain.Entities;

public sealed class PaymentLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsSuccess { get; set; }
    public string? Rrn { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
