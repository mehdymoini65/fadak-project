using MediatR;
using PaymentService.Application.Common;
using PaymentService.Application.Dtos;

namespace PaymentService.Application.Features.Payments.VerifyPayment;

public sealed record VerifyPaymentCommand(VerifyPaymentRequest Request)
    : IRequest<EndpointResult<VerifyPaymentResponse>>;
