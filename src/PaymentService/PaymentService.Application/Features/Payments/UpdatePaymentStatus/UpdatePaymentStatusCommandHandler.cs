using MediatR;
using PaymentService.Application.Common;
using PaymentService.Application.Contracts;
using PaymentService.Application.Dtos;

namespace PaymentService.Application.Features.Payments.UpdatePaymentStatus;

public sealed class UpdatePaymentStatusCommandHandler(ITransactionService transactionService)
    : IRequestHandler<UpdatePaymentStatusCommand, EndpointResult<UpdatePaymentStatusResponse>>
{
    public Task<EndpointResult<UpdatePaymentStatusResponse>> Handle(
        UpdatePaymentStatusCommand request,
        CancellationToken cancellationToken) =>
        transactionService.UpdateStatusAsync(request.Request, cancellationToken);
}
