namespace PaymentService.Application.Dtos;

public sealed class GetTokenResponse
{
    public bool IsSuccess { get; set; }
    public string GatewayUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
