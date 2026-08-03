using MediatR;
using PaymentService.Application.Common;
using PaymentService.Application.Contracts;
using PaymentService.Application.Dtos;

namespace PaymentService.Application.Features.Payments.GetTransactionInfo;

public sealed class GetTransactionInfoQueryHandler(ITransactionService transactionService)
    : IRequestHandler<GetTransactionInfoQuery, EndpointResult<TransactionInfoResponse>>
{
    public Task<EndpointResult<TransactionInfoResponse>> Handle(
        GetTransactionInfoQuery request,
        CancellationToken cancellationToken) =>
        transactionService.GetInfoAsync(request.Token, cancellationToken);
}
