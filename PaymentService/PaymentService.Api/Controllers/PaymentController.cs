using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Common;
using PaymentService.Application.Contracts;
using PaymentService.Application.Dtos;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/payment")]
public sealed class PaymentController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public PaymentController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Creates a pending transaction and returns the payment token along with the
    /// gateway URL the terminal must redirect the shopper to.
    /// </summary>
    [HttpPost("get-token")]
    [ProducesResponseType(typeof(GetTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetToken([FromBody] GetTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.GetTokenAsync(request, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Verifies a transaction: checks the token, enforces the expiry window and
    /// records the gateway app code. Returns the current status and payment data.
    /// </summary>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(VerifyPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verify([FromBody] VerifyPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.VerifyAsync(request, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Internal endpoint called by the gateway with the terminal result.
    /// Atomically moves the transaction to Success/Failed, stores the RRN and
    /// publishes a PaymentProcessedEvent. Idempotent on repeated calls.
    /// </summary>
    [HttpPost("update-status")]
    [ProducesResponseType(typeof(UpdatePaymentStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdatePaymentStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.UpdateStatusAsync(request, cancellationToken);
        return MapResult(result);
    }

    /// <summary>Returns the full transaction details for a token (used by the gateway).</summary>
    [HttpGet("info/{token}")]
    [ProducesResponseType(typeof(TransactionInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInfo([FromRoute] string token, CancellationToken cancellationToken)
    {
        var result = await _transactionService.GetInfoAsync(token, cancellationToken);
        return MapResult(result);
    }

    private IActionResult MapResult<T>(EndpointResult<T> result)
        where T : class
    {
        if (result.StatusCode == StatusCodes.Status200OK && result.Response is not null)
        {
            return Ok(result.Response);
        }

        return BadRequest(new ErrorResponse
        {
            IsSuccess = false,
            Message = result.Error ?? Messages.ValidationFailed
        });
    }
}
