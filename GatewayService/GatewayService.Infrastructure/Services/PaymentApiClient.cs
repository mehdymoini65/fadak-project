using System.Net.Http.Json;
using GatewayService.Application.Contracts;
using GatewayService.Application.Dtos;
using GatewayService.Application.Options;
using Microsoft.Extensions.Options;

namespace GatewayService.Infrastructure.Services;

public sealed class PaymentApiClient(HttpClient client, IOptions<PaymentServiceOptions> options) : IPaymentApiClient
{
    private readonly PaymentServiceOptions _options = options.Value;

    public async Task<TransactionInfoResponse?> GetInfoAsync(string token, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"/api/payment/info/{Uri.EscapeDataString(token)}", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TransactionInfoResponse>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateStatusAsync(UpdatePaymentStatusRequest request, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(_options.UpdateStatusPath, request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
