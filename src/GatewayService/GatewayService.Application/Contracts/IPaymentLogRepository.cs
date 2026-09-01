using GatewayService.Domain.Entities;
namespace GatewayService.Application.Contracts;
public interface IPaymentLogRepository
{
    Task AddAsync(PaymentLog paymentLog, CancellationToken cancellationToken);
}
