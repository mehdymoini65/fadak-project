using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Common;
using PaymentService.Application.Dtos;
using PaymentService.Application.Features.Payments.GetToken;
using PaymentService.Application.Features.Payments.GetTransactionInfo;
using PaymentService.Application.Features.Payments.UpdatePaymentStatus;
using PaymentService.Application.Features.Payments.VerifyPayment;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/payment")]
public sealed class PaymentController(IMediator mediator) : ControllerBase
{
    [HttpPost("get-token")]
    [ProducesResponseType(typeof(GetTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetToken([FromBody] GetTokenRequest request, CancellationToken cancellationToken)
        => MapResult(await mediator.Send(new GetTokenCommand(request), cancellationToken));

    [HttpPost("verify")]
    [ProducesResponseType(typeof(VerifyPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verify([FromBody] VerifyPaymentRequest request, CancellationToken cancellationToken)
        => MapResult(await mediator.Send(new VerifyPaymentCommand(request), cancellationToken));

    [HttpPost("update-status")]
    [ProducesResponseType(typeof(UpdatePaymentStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdatePaymentStatusRequest request, CancellationToken cancellationToken)
        => MapResult(await mediator.Send(new UpdatePaymentStatusCommand(request), cancellationToken));

    [HttpGet("info/{token}")]
    [ProducesResponseType(typeof(TransactionInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInfo([FromRoute] string token, CancellationToken cancellationToken)
        => MapResult(await mediator.Send(new GetTransactionInfoQuery(token), cancellationToken));

    private IActionResult MapResult<T>(EndpointResult<T> result) where T : class
    {
        if (result.StatusCode == StatusCodes.Status200OK && result.Response is not null)
            return Ok(result.Response);

        return BadRequest(new ErrorResponse
        {
            IsSuccess = false,
            Message = result.Error ?? Messages.ValidationFailed
        });
    }
}
