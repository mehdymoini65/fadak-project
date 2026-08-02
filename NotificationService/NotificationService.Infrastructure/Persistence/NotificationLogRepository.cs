using NotificationService.Application;
using NotificationService.Domain.Entities;
namespace NotificationService.Infrastructure.Persistence;
public sealed class NotificationLogRepository(NotificationDbContext db) : INotificationLogRepository
{
    public async Task AddAsync(NotificationLog log, CancellationToken cancellationToken) { db.NotificationLogs.Add(log); await db.SaveChangesAsync(cancellationToken); }
}
