using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace GatewayService.Infrastructure.Persistence;
public sealed class DatabaseInitializer(IServiceProvider services, ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try { using var scope = services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<GatewayDbContext>(); await db.Database.EnsureCreatedAsync(cancellationToken); }
        catch (Exception ex) { logger.LogError(ex, "Gateway database initialization failed."); }
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
