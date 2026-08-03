using MediatR;
using PaymentService.Application.Common;
using PaymentService.Application.Contracts;
using PaymentService.Application.Dtos;

namespace PaymentService.Application.Features.Payments.VerifyPayment;

public sealed class VerifyPaymentCommandHandler(ITransactionService transactionService)
    : IRequestHandler<VerifyPaymentCommand, EndpointResult<VerifyPaymentResponse>>
{
    public Task<EndpointResult<VerifyPaymentResponse>> Handle(
        VerifyPaymentCommand request,
        CancellationToken cancellationToken) =>
        transactionService.VerifyAsync(request.Request, cancellationToken);
}
