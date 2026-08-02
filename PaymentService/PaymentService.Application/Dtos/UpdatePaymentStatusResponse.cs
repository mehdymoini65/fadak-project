namespace PaymentService.Application.Dtos;

public sealed class UpdatePaymentStatusResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
