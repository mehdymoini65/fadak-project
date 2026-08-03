using MediatR;
using PaymentService.Application.Common;
using PaymentService.Application.Contracts;
using PaymentService.Application.Dtos;

namespace PaymentService.Application.Features.Payments.GetToken;

public sealed class GetTokenCommandHandler(ITransactionService transactionService)
    : IRequestHandler<GetTokenCommand, EndpointResult<GetTokenResponse>>
{
    public Task<EndpointResult<GetTokenResponse>> Handle(
        GetTokenCommand request,
        CancellationToken cancellationToken) =>
        transactionService.GetTokenAsync(request.Request, cancellationToken);
}
