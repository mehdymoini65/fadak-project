using NotificationService.Infrastructure;
var builder=WebApplication.CreateBuilder(args); builder.Services.AddNotificationInfrastructure(builder.Configuration); builder.Services.AddHealthChecks();
var app=builder.Build(); app.MapHealthChecks("/health"); app.MapGet("/",()=>Results.Ok(new{service="NotificationService",status="running"})); app.Run();
