using MediatR;
using PaymentService.Application.Common;
using PaymentService.Application.Dtos;

namespace PaymentService.Application.Features.Payments.UpdatePaymentStatus;

public sealed record UpdatePaymentStatusCommand(UpdatePaymentStatusRequest Request)
    : IRequest<EndpointResult<UpdatePaymentStatusResponse>>;
