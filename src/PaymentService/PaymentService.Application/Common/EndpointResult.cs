using Microsoft.AspNetCore.Http;

namespace PaymentService.Application.Common;

/// <summary>
/// Outcome returned by application services. When <see cref="StatusCode"/> is 200
/// the controller returns <see cref="Response"/> as the body; any other status
/// makes the controller return an <see cref="ErrorResponse"/> built from <see cref="Error"/>.
/// </summary>
public sealed class EndpointResult<T>
    where T : class
{
    public int StatusCode { get; init; } = StatusCodes.Status200OK;
    public T? Response { get; init; }
    public string? Error { get; init; }

    public static EndpointResult<T> Success(T response) =>
        new() { StatusCode = StatusCodes.Status200OK, Response = response };

    public static EndpointResult<T> Failure(int statusCode, string error) =>
        new() { StatusCode = statusCode, Error = error };

    public static EndpointResult<T> Invalid(string error) =>
        Failure(StatusCodes.Status400BadRequest, error);
}
