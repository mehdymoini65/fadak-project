namespace PaymentService.Application.Options;

/// <summary>
/// Settings for the background job that expires pending transactions.
/// </summary>
public sealed class PaymentExpirationOptions
{
    public const string SectionName = "PaymentExpiration";

    /// <summary>How often the expiration job runs (seconds).</summary>
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>Pending transactions older than this (minutes) are marked as expired.</summary>
    public int TimeoutMinutes { get; set; } = 2;
}
