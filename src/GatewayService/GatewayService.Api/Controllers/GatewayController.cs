using GatewayService.Application.Dtos;
using GatewayService.Application.Contracts;
using GatewayService.Domain.Entities;

using Microsoft.AspNetCore.Mvc;

namespace GatewayService.Api.Controllers;

[ApiController]
[Route("api/gateway")]
public sealed class GatewayController : ControllerBase
{
    private const int SuccessRatePercent = 80;

    private readonly IPaymentApiClient _paymentApiClient;
    private readonly IPaymentLogRepository _paymentLogRepository;
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(IPaymentApiClient paymentApiClient, IPaymentLogRepository paymentLogRepository, ILogger<GatewayController> logger)
    {
        _paymentApiClient = paymentApiClient;
        _paymentLogRepository = paymentLogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Simulates the bank payment page. ~80% of payments succeed and a random
    /// 12-digit RRN is issued; otherwise the payment fails without an RRN. The
    /// terminal result is forwarded to the payment service and the outcome is
    /// returned to the shopper.
    /// </summary>
    [HttpGet("pay/{token}")]
    [ProducesResponseType(typeof(GatewayPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GatewayPaymentResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Pay([FromRoute] string token, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Simulating bank payment page for token {Token}.", token);

        var transaction = await _paymentApiClient.GetInfoAsync(token, cancellationToken);

        if (transaction is null)
        {
            return BadRequest(new GatewayPaymentResponse
            {
                IsSuccess = false,
                Message = "توکن نامعتبر است"
            });
        }

        if (transaction.Status != PaymentStatus.Pending)
        {
            return BadRequest(new GatewayPaymentResponse
            {
                IsSuccess = false,
                Token = token,
                Amount = transaction.Amount,
                RedirectUrl = transaction.RedirectUrl,
                Message = "تراکنش قبلاً پردازش شده است"
            });
        }

        // Simulate bank processing time.
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        var isSuccess = Random.Shared.Next(100) < SuccessRatePercent;
        var rrn = isSuccess ? GenerateRrn() : null;

        _logger.LogInformation(
            "Simulation result for token {Token}: {Result} (RRN: {Rrn}).",
            token,
            isSuccess ? "Success" : "Failed",
            rrn ?? "-");

        var recorded = await _paymentApiClient.UpdateStatusAsync(
            new UpdatePaymentStatusRequest
            {
                Token = token,
                IsSuccess = isSuccess,
                Rrn = rrn
            },
            cancellationToken);

        if (!recorded)
        {
            return BadRequest(new GatewayPaymentResponse
            {
                IsSuccess = false,
                Token = token,
                Amount = transaction.Amount,
                RedirectUrl = transaction.RedirectUrl,
                Message = "خطا در ثبت نتیجه پرداخت"
            });
        }

        await _paymentLogRepository.AddAsync(new PaymentLog
        {
            Token = token,
            Amount = transaction.Amount,
            IsSuccess = isSuccess,
            Rrn = rrn,
            ProcessedAt = DateTime.UtcNow
        }, cancellationToken);

        return Ok(new GatewayPaymentResponse
        {
            IsSuccess = isSuccess,
            Token = token,
            Rrn = rrn,
            Amount = transaction.Amount,
            RedirectUrl = transaction.RedirectUrl,
            Message = isSuccess ? "پرداخت با موفقیت انجام شد" : "پرداخت ناموفق بود"
        });

        static string GenerateRrn() =>
            Random.Shared.NextInt64(1_000_000_000_00, 9_999_999_999_99).ToString();
    }
}
