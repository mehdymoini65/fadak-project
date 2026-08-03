using MediatR;
using PaymentService.Application.Common;
using PaymentService.Application.Dtos;

namespace PaymentService.Application.Features.Payments.GetToken;

public sealed record GetTokenCommand(GetTokenRequest Request)
    : IRequest<EndpointResult<GetTokenResponse>>;
