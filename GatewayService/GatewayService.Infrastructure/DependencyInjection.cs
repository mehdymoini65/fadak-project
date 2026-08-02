using GatewayService.Application.Contracts;
using GatewayService.Application.Options;
using GatewayService.Infrastructure.Persistence;
using GatewayService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace GatewayService.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("GatewayDatabase") ?? configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Gateway database connection string is missing.");
        services.AddDbContext<GatewayDbContext>(o => o.UseSqlServer(cs));
        services.AddScoped<IPaymentLogRepository, PaymentLogRepository>();
        services.AddHostedService<DatabaseInitializer>();
        services.Configure<PaymentServiceOptions>(configuration.GetSection(PaymentServiceOptions.SectionName));
        services.AddHttpClient<IPaymentApiClient, PaymentApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PaymentServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        return services;
    }
}
