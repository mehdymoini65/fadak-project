using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GatewayService.Infrastructure.Persistence;

public sealed class DatabaseInitializer(IServiceProvider services, ILogger<DatabaseInitializer> logger) : IHostedService
{
    private const int MaxAttempts = 10;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
                await db.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Gateway database migrations applied.");
                return;
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                logger.LogWarning(ex, "Gateway database is not ready. Retry {Attempt}/{MaxAttempts}.", attempt, MaxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }

        using var finalScope = services.CreateScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<GatewayDbContext>();
        await finalDb.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
