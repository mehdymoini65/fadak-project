using NotificationService.Domain.Entities;
namespace NotificationService.Application;
public interface INotificationLogRepository { Task AddAsync(NotificationLog log, CancellationToken cancellationToken); }
