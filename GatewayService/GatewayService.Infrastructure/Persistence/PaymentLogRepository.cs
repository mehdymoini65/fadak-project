using GatewayService.Application.Contracts;
using GatewayService.Domain.Entities;
namespace GatewayService.Infrastructure.Persistence;
public sealed class PaymentLogRepository(GatewayDbContext db) : IPaymentLogRepository
{
    public async Task AddAsync(PaymentLog paymentLog, CancellationToken cancellationToken)
    {
        db.PaymentLogs.Add(paymentLog);
        await db.SaveChangesAsync(cancellationToken);
    }
}
