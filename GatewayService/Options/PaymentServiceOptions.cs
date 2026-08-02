namespace GatewayService.Options;

/// <summary>
/// Settings for the internal calls made to the payment service.
/// </summary>
public sealed class PaymentServiceOptions
{
    public const string SectionName = "PaymentService";

    public string BaseUrl { get; set; } = "http://localhost:5001";

    public string UpdateStatusPath { get; set; } = "/api/payment/update-status";

    public int TimeoutSeconds { get; set; } = 30;
}
