using PaymentService.Application.Common;
using PaymentService.Application.Dtos;

namespace PaymentService.Application.Contracts;

/// <summary>
/// Orchestrates the payment lifecycle: token issuance, verification and the
/// gateway-reported result. All state changes go through atomic transitions so
/// concurrent calls (gateway callback, verify, expiration job) stay consistent.
/// </summary>
public interface ITransactionService
{
    Task<EndpointResult<GetTokenResponse>> GetTokenAsync(GetTokenRequest request, CancellationToken cancellationToken);

    Task<EndpointResult<VerifyPaymentResponse>> VerifyAsync(VerifyPaymentRequest request, CancellationToken cancellationToken);

    Task<EndpointResult<UpdatePaymentStatusResponse>> UpdateStatusAsync(UpdatePaymentStatusRequest request, CancellationToken cancellationToken);

    Task<EndpointResult<TransactionInfoResponse>> GetInfoAsync(string token, CancellationToken cancellationToken);
}
