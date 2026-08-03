using MediatR;
using PaymentService.Application.Common;
using PaymentService.Application.Dtos;

namespace PaymentService.Application.Features.Payments.GetTransactionInfo;

public sealed record GetTransactionInfoQuery(string Token)
    : IRequest<EndpointResult<TransactionInfoResponse>>;
