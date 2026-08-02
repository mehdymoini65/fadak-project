namespace GatewayService.Dtos;

/// <summary>
/// What the gateway returns to the shopper/frontend after simulating the
/// bank payment page.
/// </summary>
public sealed class GatewayPaymentResponse
{
    public bool IsSuccess { get; set; }
    public string Token { get; set; } = string.Empty;
    public string? Rrn { get; set; }
    public decimal Amount { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
}
