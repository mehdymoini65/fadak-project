using Microsoft.EntityFrameworkCore; using Microsoft.Extensions.DependencyInjection; using Microsoft.Extensions.Hosting; using Microsoft.Extensions.Logging;
namespace NotificationService.Infrastructure.Persistence;
public sealed class DatabaseInitializer(IServiceProvider services, ILogger<DatabaseInitializer> logger) : IHostedService
{
 public async Task StartAsync(CancellationToken ct) { try { using var s=services.CreateScope(); var db=s.ServiceProvider.GetRequiredService<NotificationDbContext>(); await db.Database.EnsureCreatedAsync(ct); } catch(Exception ex){ logger.LogError(ex,"Notification database initialization failed."); } }
 public Task StopAsync(CancellationToken ct)=>Task.CompletedTask;
}
