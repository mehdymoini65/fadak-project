namespace PaymentService.Domain.Abstractions;

/// <summary>
/// Thrown when persisting a transaction fails because its generated token
/// collided with an existing unique token.
/// </summary>
public sealed class DuplicateTokenException : Exception
{
    public string Token { get; }

    public DuplicateTokenException(string token, Exception innerException)
        : base("A transaction with the same token already exists.", innerException)
    {
        Token = token;
    }
}
