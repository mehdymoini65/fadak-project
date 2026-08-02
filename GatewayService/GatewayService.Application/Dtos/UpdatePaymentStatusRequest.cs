namespace GatewayService.Application.Dtos;

/// <summary>
/// Payload sent to the payment service to record the gateway result.
/// Mirrors the payment service's update-status request (token/isSuccess/rrn).
/// </summary>
public sealed class UpdatePaymentStatusRequest
{
    public string Token { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? Rrn { get; set; }
}
