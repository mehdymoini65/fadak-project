using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application;
using NotificationService.Application.Options;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Services;
namespace NotificationService.Infrastructure;
public static class DependencyInjection
{
 public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
 {
  var cs=configuration.GetConnectionString("NotificationDatabase") ?? configuration["Database:ConnectionString"] ?? throw new InvalidOperationException("Notification database connection string is missing.");
  services.AddDbContext<NotificationDbContext>(o=>o.UseSqlServer(cs)); services.AddScoped<INotificationLogRepository,NotificationLogRepository>(); services.AddHostedService<DatabaseInitializer>();
  services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName)); services.Configure<CallbackOptions>(configuration.GetSection(CallbackOptions.SectionName));
  services.AddHttpClient("callback",(sp,c)=>{var o=sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CallbackOptions>>().Value;c.Timeout=TimeSpan.FromSeconds(Math.Max(1,o.TimeoutSeconds));});
  services.AddHostedService<PaymentEventConsumer>(); return services;
 }
}
