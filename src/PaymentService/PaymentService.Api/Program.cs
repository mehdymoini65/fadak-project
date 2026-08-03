using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PaymentService.Application.Common;
using PaymentService.Application.Features.Payments.GetToken;
using PaymentService.Infrastructure;
using PaymentService.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var error = context.ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
            ?? Messages.ValidationFailed;

        return new BadRequestObjectResult(new ErrorResponse { IsSuccess = false, Message = error });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fadak Payment Service API",
        Version = "v1",
        Description = "Payment token creation, verification and internal status update APIs."
    });
});

builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(GetTokenCommand).Assembly));

builder.Services.AddPaymentInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service v1");
    options.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();
