using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Contracts;
using PaymentService.Application.Options;
using PaymentService.Application.Services;
using PaymentService.Domain.Abstractions;
using PaymentService.Infrastructure.Middleware;
using PaymentService.Infrastructure.MessageBus;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Scheduling;
using Quartz;

namespace PaymentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<GatewayOptions>(configuration.GetSection(GatewayOptions.SectionName));
        services.Configure<PaymentExpirationOptions>(configuration.GetSection(PaymentExpirationOptions.SectionName));

        // Database
        services.AddDbContext<PaymentDbContext>(options =>
        {
            var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
            var connectionString = configuration.GetConnectionString("PaymentDatabase") ?? dbOptions.ConnectionString;
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddHostedService<DatabaseInitializer>();

        // Message bus
        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        // Application services
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ITransactionExpirationService, TransactionExpirationService>();

        // Middleware
        services.AddSingleton<ExceptionHandlingMiddleware>();

        // Quartz background jobs
        ConfigureQuartz(services, configuration);

        return services;
    }

    private static void ConfigureQuartz(IServiceCollection services, IConfiguration configuration)
    {
        var expirationOptions = configuration
            .GetSection(PaymentExpirationOptions.SectionName)
            .Get<PaymentExpirationOptions>()
            ?? new PaymentExpirationOptions();

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(TransactionExpirationJob.JobName, TransactionExpirationJob.GroupName);
            q.AddJob<TransactionExpirationJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("TransactionExpirationTrigger", TransactionExpirationJob.GroupName)
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(Math.Max(1, expirationOptions.IntervalSeconds))
                    .RepeatForever())
                .StartNow());
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }
}
