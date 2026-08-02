namespace PaymentService.Application.Options;

/// <summary>
/// Settings used to build the gateway URL returned by get-token.
/// </summary>
public sealed class GatewayOptions
{
    public const string SectionName = "Gateway";

    public string BaseUrl { get; set; } = "https://localhost:5002";
}
