using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaymentService.Infrastructure.Persistence;

public sealed class DatabaseInitializer : IHostedService
{
    private const int MaxAttempts = 10;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                await db.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Payment database migrations applied.");
                return;
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                _logger.LogWarning(ex, "Payment database is not ready. Retry {Attempt}/{MaxAttempts}.", attempt, MaxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }

        using var finalScope = _serviceProvider.CreateScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await finalDb.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
