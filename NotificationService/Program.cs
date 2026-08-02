using NotificationService.Options;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<CallbackOptions>(builder.Configuration.GetSection(CallbackOptions.SectionName));
builder.Services.AddHttpClient("callback", (sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CallbackOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds));
});
builder.Services.AddHostedService<PaymentEventConsumer>();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { service = "NotificationService", status = "running" }));
app.Run();
