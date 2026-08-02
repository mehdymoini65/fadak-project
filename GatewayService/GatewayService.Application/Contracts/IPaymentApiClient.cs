using GatewayService.Application.Dtos;
namespace GatewayService.Application.Contracts;
public interface IPaymentApiClient
{
    Task<TransactionInfoResponse?> GetInfoAsync(string token, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(UpdatePaymentStatusRequest request, CancellationToken cancellationToken);
}
