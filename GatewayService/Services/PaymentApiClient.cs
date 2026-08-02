using System.Net.Http.Json;
using GatewayService.Dtos;
using GatewayService.Options;
using Microsoft.Extensions.Options;

namespace GatewayService.Services;

public interface IPaymentApiClient
{
    Task<TransactionInfoResponse?> GetInfoAsync(string token, CancellationToken cancellationToken);

    Task<bool> UpdateStatusAsync(UpdatePaymentStatusRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// HTTP client that talks to the internal payment service to look up a
/// transaction and to record the simulated terminal result.
/// </summary>
public sealed class PaymentApiClient : IPaymentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly PaymentServiceOptions _options;
    private readonly ILogger<PaymentApiClient> _logger;

    public PaymentApiClient(
        HttpClient httpClient,
        IOptions<PaymentServiceOptions> options,
        ILogger<PaymentApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TransactionInfoResponse?> GetInfoAsync(string token, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/payment/info/{Uri.EscapeDataString(token)}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Payment service returned HTTP {StatusCode} when looking up token {Token}.",
                (int)response.StatusCode,
                token);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TransactionInfoResponse>(cancellationToken);
    }

    public async Task<bool> UpdateStatusAsync(UpdatePaymentStatusRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(_options.UpdateStatusPath, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Payment service returned HTTP {StatusCode} for update-status (token {Token}).",
                (int)response.StatusCode,
                request.Token);
            return false;
        }

        return true;
    }
}
