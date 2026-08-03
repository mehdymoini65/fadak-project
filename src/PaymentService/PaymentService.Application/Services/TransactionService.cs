using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.Common;
using PaymentService.Application.Contracts;
using PaymentService.Application.Dtos;
using PaymentService.Application.Options;
using PaymentService.Domain.Abstractions;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Events;

namespace PaymentService.Application.Services;

public sealed class TransactionService : ITransactionService
{
    private const int MaxTokenCollisionRetries = 3;

    private readonly ITransactionRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly GatewayOptions _gatewayOptions;
    private readonly PaymentExpirationOptions _expirationOptions;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository repository,
        IEventBus eventBus,
        IOptions<GatewayOptions> gatewayOptions,
        IOptions<PaymentExpirationOptions> expirationOptions,
        ILogger<TransactionService> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _gatewayOptions = gatewayOptions.Value;
        _expirationOptions = expirationOptions.Value;
        _logger = logger;
    }

    public async Task<EndpointResult<GetTokenResponse>> GetTokenAsync(GetTokenRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return EndpointResult<GetTokenResponse>.Invalid(Messages.AmountInvalid);
        }

        for (var attempt = 0; attempt < MaxTokenCollisionRetries; attempt++)
        {
            var transaction = new Transaction(
                request.TerminalNo,
                request.Amount,
                request.RedirectUrl,
                request.ReservationNumber,
                request.PhoneNumber);

            try
            {
                await _repository.AddAsync(transaction, cancellationToken);

                _logger.LogInformation(
                    "Transaction {TransactionId} created with token {Token}, terminal {TerminalNo}.",
                    transaction.Id,
                    transaction.Token,
                    transaction.TerminalNo);

                var gatewayUrl = $"{_gatewayOptions.BaseUrl.TrimEnd('/')}/api/gateway/pay/{transaction.Token}";

                return EndpointResult<GetTokenResponse>.Success(new GetTokenResponse
                {
                    IsSuccess = true,
                    GatewayUrl = gatewayUrl,
                    Token = transaction.Token,
                    Message = Messages.TokenCreated
                });
            }
            catch (DuplicateTokenException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Token collision while persisting transaction, retrying ({Attempt}/{Max}).",
                    attempt + 1,
                    MaxTokenCollisionRetries);
            }
        }

        return EndpointResult<GetTokenResponse>.Invalid(Messages.InternalError);
    }

    public async Task<EndpointResult<VerifyPaymentResponse>> VerifyAsync(VerifyPaymentRequest request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByTokenAsync(request.Token, cancellationToken);

        if (transaction is null)
        {
            return EndpointResult<VerifyPaymentResponse>.Invalid(Messages.InvalidToken);
        }

        // The specification requires AppCode to be persisted during Verify.
        await _repository.UpdateAppCodeAsync(request.Token, request.AppCode, cancellationToken);
        _logger.LogInformation("AppCode recorded for token {Token}.", request.Token);

        var cutoff = DateTime.UtcNow.AddMinutes(-_expirationOptions.TimeoutMinutes);
        var timeExpired = transaction.CreatedAt < cutoff;

        if (transaction.Status == PaymentStatus.Expired || timeExpired)
        {
            if (timeExpired && transaction.Status == PaymentStatus.Pending)
            {
                await _repository.TryExpireAsync(request.Token, cutoff, cancellationToken);
            }

            return EndpointResult<VerifyPaymentResponse>.Success(new VerifyPaymentResponse
            {
                IsSuccess = false,
                Status = nameof(PaymentStatus.Expired),
                Amount = transaction.Amount,
                ReservationNumber = transaction.ReservationNumber,
                Message = Messages.PaymentExpired
            });
        }

        if (transaction.Status == PaymentStatus.Success)
        {
            return EndpointResult<VerifyPaymentResponse>.Success(new VerifyPaymentResponse
            {
                IsSuccess = true,
                Status = nameof(PaymentStatus.Success),
                Amount = transaction.Amount,
                Rrn = transaction.Rrn,
                ReservationNumber = transaction.ReservationNumber,
                Message = Messages.PaymentVerified
            });
        }

        if (transaction.Status == PaymentStatus.Failed)
        {
            return EndpointResult<VerifyPaymentResponse>.Success(new VerifyPaymentResponse
            {
                IsSuccess = false,
                Status = nameof(PaymentStatus.Failed),
                Amount = transaction.Amount,
                ReservationNumber = transaction.ReservationNumber,
                Message = Messages.PaymentFailed
            });
        }

        // Pending is not part of the Verify response contract defined in the task document.
        return EndpointResult<VerifyPaymentResponse>.Invalid(Messages.PaymentPending);
    }

    public async Task<EndpointResult<UpdatePaymentStatusResponse>> UpdateStatusAsync(UpdatePaymentStatusRequest request, CancellationToken cancellationToken)
    {
        var targetStatus = request.IsSuccess ? PaymentStatus.Success : PaymentStatus.Failed;

        if (request.IsSuccess && string.IsNullOrWhiteSpace(request.Rrn))
        {
            return EndpointResult<UpdatePaymentStatusResponse>.Invalid(Messages.RrnRequired);
        }

        if (request.Rrn is not null && request.Rrn.Length != 12)
        {
            return EndpointResult<UpdatePaymentStatusResponse>.Invalid(Messages.InvalidRrn);
        }

        var result = await _repository.TryTransitionAsync(
            request.Token,
            PaymentStatus.Pending,
            targetStatus,
            request.Rrn,
            cancellationToken);

        if (result.Transaction is null)
        {
            return EndpointResult<UpdatePaymentStatusResponse>.Invalid(Messages.InvalidToken);
        }

        var transaction = result.Transaction;

        if (!result.Changed)
        {
            var message = transaction.Status == targetStatus
                ? Messages.StatusAlreadyUpdated
                : Messages.StatusUnchangeable;

            _logger.LogInformation(
                "update-status for token {Token} had no effect; current status {Status}.",
                request.Token,
                transaction.Status);

            return EndpointResult<UpdatePaymentStatusResponse>.Success(new UpdatePaymentStatusResponse
            {
                IsSuccess = true,
                Message = message
            });
        }

        await PublishAsync(
            new PaymentProcessedEvent(
                transaction.Id,
                transaction.Token,
                transaction.Status,
                transaction.Amount,
                transaction.Rrn,
                transaction.ReservationNumber,
                transaction.RedirectUrl,
                DateTimeOffset.UtcNow),
            cancellationToken);

        _logger.LogInformation(
            "Transaction {TransactionId} moved to {Status} with RRN {Rrn}.",
            transaction.Id,
            transaction.Status,
            transaction.Rrn);

        return EndpointResult<UpdatePaymentStatusResponse>.Success(new UpdatePaymentStatusResponse
        {
            IsSuccess = true,
            Message = Messages.StatusUpdated
        });
    }

    public async Task<EndpointResult<TransactionInfoResponse>> GetInfoAsync(string token, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByTokenAsync(token, cancellationToken);

        if (transaction is null)
        {
            return EndpointResult<TransactionInfoResponse>.Invalid(Messages.InvalidToken);
        }

        return EndpointResult<TransactionInfoResponse>.Success(new TransactionInfoResponse
        {
            Token = transaction.Token,
            Status = transaction.Status,
            Amount = transaction.Amount,
            Rrn = transaction.Rrn,
            AppCode = transaction.AppCode,
            TerminalNo = transaction.TerminalNo,
            ReservationNumber = transaction.ReservationNumber,
            PhoneNumber = transaction.PhoneNumber,
            RedirectUrl = transaction.RedirectUrl,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        });
    }

    private async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class
    {
        try
        {
            await _eventBus.PublishAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish integration event {EventType}.", typeof(TEvent).Name);
            throw;
        }
    }

}
