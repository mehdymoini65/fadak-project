namespace PaymentService.Application.Common;

/// <summary>
/// Minimal error envelope returned for 400-style responses.
/// </summary>
public sealed class ErrorResponse
{
    public bool IsSuccess { get; init; } = false;
    public string Message { get; init; } = string.Empty;
}
