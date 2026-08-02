using Microsoft.OpenApi.Models;
using NotificationService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fadak Notification Service API",
        Version = "v1",
        Description = "Payment event consumer health and service status endpoints."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service v1");
    options.RoutePrefix = "swagger";
});

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { service = "NotificationService", status = "running" }))
    .WithName("GetNotificationServiceStatus")
    .WithOpenApi();

app.Run();
